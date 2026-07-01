using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TaskEasy.Services
{
    public class PayPalService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _secret;

        public PayPalService(IHttpClientFactory httpFactory, IConfiguration configuration)
        {
            _httpFactory = httpFactory;
            var cfg = configuration.GetSection("PayPal");
            _baseUrl = cfg.GetValue<string>("ApiBaseUrl") ?? "https://api-m.sandbox.paypal.com";
            _clientId = cfg.GetValue<string>("ClientId") ?? throw new ArgumentNullException("PayPal:ClientId");
            _secret = cfg.GetValue<string>("Secret") ?? throw new ArgumentNullException("PayPal:Secret");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_baseUrl);

            var byteArray = Encoding.UTF8.GetBytes($"{_clientId}:{_secret}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });
            var response = await client.PostAsync("/v1/oauth2/token", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("access_token", out var tokenEl))
            {
                return tokenEl.GetString()!;
            }

            throw new InvalidOperationException("Could not obtain access token from PayPal");
        }

        public async Task<string> CreateOrderAsync(decimal amount, string currency = "USD")
        {
            var token = await GetAccessTokenAsync();
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var order = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new {
                        amount = new {
                            currency_code = currency,
                            value = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(order);
            var response = await client.PostAsync("/v2/checkout/orders", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var respJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);
            if (doc.RootElement.TryGetProperty("id", out var idEl))
            {
                return idEl.GetString()!;
            }

            throw new InvalidOperationException("Could not create PayPal order");
        }

        public async Task<JsonElement> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"/v2/checkout/orders/{orderId}/capture", new StringContent("{}", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var respJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);
            return doc.RootElement.Clone();
        }

        public async Task<bool> VerifyWebhookSignatureAsync(string transmissionId, string transmissionTime, string certUrl, string authAlgo, string transmissionSig, string webhookId, string body)
        {
            // If webhookId is not configured, we cannot verify
            if (string.IsNullOrEmpty(webhookId)) return false;

            var token = await GetAccessTokenAsync();
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                auth_algo = authAlgo,
                cert_url = certUrl,
                transmission_id = transmissionId,
                transmission_sig = transmissionSig,
                transmission_time = transmissionTime,
                webhook_id = webhookId,
                webhook_event = JsonDocument.Parse(body).RootElement
            };

            var json = JsonSerializer.Serialize(payload);
            var resp = await client.PostAsync("/v1/notifications/verify-webhook-signature", new StringContent(json, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode) return false;
            var respJson = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);
            if (doc.RootElement.TryGetProperty("verification_status", out var v))
            {
                return v.GetString() == "SUCCESS";
            }
            return false;
        }
    }
}
