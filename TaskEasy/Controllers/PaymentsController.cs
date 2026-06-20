using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEasy.Data;
using TaskEasy.Models;
using TaskEasy.Models.DTOs;

namespace TaskEasy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const decimal PRECIO_PRO = 4999;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET
        [HttpGet("plan")]
        public async Task<IActionResult> GetPlanActual()
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new { user.Plan, user.PlanExpira, precioPro = PRECIO_PRO });
        }

        // GET: api/payments/historial
        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial()
        {
            var userId = GetUserId();
            var pagos = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return Ok(pagos);
        }

        // POST
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDTO dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            bool aprobado = int.Parse(dto.NumeroTarjeta[^1].ToString()) % 2 == 0;

            var pago = new Payment
            {
                UserId = userId,
                Monto = PRECIO_PRO,
                Metodo = dto.Metodo,
                Estado = aprobado ? "Aprobado" : "Rechazado",
                Fecha = DateTime.Now
            };

            _context.Payments.Add(pago);

            if (aprobado)
            {
                user.Plan = "Pro";
                user.PlanExpira = DateTime.Now.AddMonths(1);
            }

            await _context.SaveChangesAsync();

            if (!aprobado)
                return BadRequest(new { mensaje = "Pago rechazado. Verifica los datos de tu tarjeta.", pago });

            return Ok(new { mensaje = "¡Pago aprobado! Ahora tienes el plan Pro.", pago, user.Plan, user.PlanExpira });
        }
    }
}