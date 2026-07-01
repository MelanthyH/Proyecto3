using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskEasy.Services;
using TaskEasy.Data;
using TaskEasy.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace TaskEasy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayPalController : ControllerBase
    {
        private readonly PayPalService _payPal;

        private readonly AppDbContext _context;

        public PayPalController(PayPalService payPal, AppDbContext context)
        {
            _payPal = payPal;
            _context = context;
        }

        public class CreateOrderRequest
        {
            public decimal Amount { get; set; }
            public string Currency { get; set; } = "USD";
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            if (req.Amount <= 0) return BadRequest("Amount must be greater than zero");
            var orderId = await _payPal.CreateOrderAsync(req.Amount, req.Currency ?? "USD");
            return Ok(new { orderId });
        }

        public class CaptureOrderRequest
        {
            public string OrderId { get; set; } = string.Empty;
        }

        [HttpPost("capture-order")]
        public async Task<IActionResult> CaptureOrder([FromBody] CaptureOrderRequest req)
        {
            if (string.IsNullOrEmpty(req.OrderId)) return BadRequest("orderId is required");

            var result = await _payPal.CaptureOrderAsync(req.OrderId);

            string status = "UNKNOWN";
            decimal monto = 0m;
            try
            {
                if (result.TryGetProperty("status", out var st)) status = st.GetString() ?? "UNKNOWN";
                if (result.TryGetProperty("purchase_units", out var pus) && pus.GetArrayLength() > 0)
                {
                    var pu = pus[0];
                    if (pu.TryGetProperty("payments", out var payments) && payments.TryGetProperty("captures", out var captures) && captures.GetArrayLength() > 0)
                    {
                        var cap = captures[0];
                        if (cap.TryGetProperty("status", out var capStatus)) status = capStatus.GetString() ?? status;
                        if (cap.TryGetProperty("amount", out var amt) && amt.TryGetProperty("value", out var val))
                        {
                            decimal.TryParse(val.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out monto);
                        }
                    }
                }
            }
            catch { }

            int userId = 0;
            try { userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); } catch { }

            var pago = new Payment
            {
                UserId = userId,
                Monto = monto > 0 ? monto : 4999m,
                Metodo = "Paypal",
                Estado = status == "COMPLETED" || status == "COMPLETED".ToUpper() ? "Aprobado" : status,
                Fecha = DateTime.Now
            };

            _context.Payments.Add(pago);
            if (userId > 0)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null && (pago.Estado == "Aprobado" || pago.Estado == "COMPLETED"))
                {
                    user.Plan = "Pro";
                    user.PlanExpira = DateTime.Now.AddMonths(1);
                }
            }

            await _context.SaveChangesAsync();

            return new JsonResult(result);
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var headers = Request.Headers;
            string transmissionId = headers["paypal-transmission-id"].ToString();
            string transmissionTime = headers["paypal-transmission-time"].ToString();
            string certUrl = headers["paypal-cert-url"].ToString();
            string authAlgo = headers["paypal-auth-algo"].ToString();
            string transmissionSig = headers["paypal-transmission-sig"].ToString();

            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var cfg = HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
            var webhookId = cfg?.GetValue<string>("PayPal:WebhookId");
            bool verified = await _payPal.VerifyWebhookSignatureAsync(transmissionId, transmissionTime, certUrl, authAlgo, transmissionSig, webhookId, body);

            return Ok(new { verified });
        }
    }
}
