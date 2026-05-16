using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using GameStoreWeb.Services;
using GameStoreWeb.Payments;

namespace GameStoreWeb.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankPaymentController : ControllerBase
    {
        private readonly IRazorpayPaymentService _razorpayService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentRepository _repo;

        public BankPaymentController(IRazorpayPaymentService razorpayService, IConfiguration configuration, IPaymentRepository repo)
        {
            _razorpayService = razorpayService;
            _configuration = configuration;
            _repo = repo;
        }

        [HttpPost("verify")]
        public IActionResult VerifyPayment([FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("razorpay_payment_id", out var pid) ||
                !payload.TryGetProperty("razorpay_order_id", out var oid) ||
                !payload.TryGetProperty("razorpay_signature", out var sig))
                return BadRequest("Missing fields");

            var paymentId = pid.GetString();
            var orderId = oid.GetString();
            var signature = sig.GetString();
            var secret = String.Empty;

            if (Environment.GetEnvironmentVariable("RENDER") != null)
            {
                secret = Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET");
            }
            else
            {
                secret = _configuration["Razorpay:KeySecret"]; // keep secret on server only
            }

            var generated = ComputeHmacSha256($"{orderId}|{paymentId}", secret);

            if (!SecureEquals(generated, signature))
                return BadRequest("Invalid signature");

            // Signature OK — mark payment as completed in DB, notify GameStoreWeb etc.
            // e.g. Save/Update Payment record: set status Completed

            return Ok(new { status = "verified" });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Receive()
        {
            // 1) Read raw body (must be RAW for HMAC)
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                body = await reader.ReadToEndAsync();

            // 2) Signature header check
            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signature))
            {
                Console.WriteLine("[Webhook] Missing signature");
                return BadRequest("Missing signature");
            }
            var secret = String.Empty;

            // 3) Compute HMAC SHA256 with your WebhookSecret
            if (Environment.GetEnvironmentVariable("RENDER") != null)
            {
                secret = Environment.GetEnvironmentVariable("RAZORPAY_WEBHOOK_SECRET");
            }
            else
            {
                secret = _configuration["Razorpay:WebhookSecret"];
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                Console.WriteLine("[Webhook] WebhookSecret not configured");
                return StatusCode(500, "Webhook secret not configured");
            }
            var computed = ComputeHmacSha256(body, secret);
            if (!SecureEquals(computed, signature))
            {
                Console.WriteLine("[Webhook] Invalid signature");
                return BadRequest("Invalid signature");
            }
            // 4) Parse event
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var eventType = root.GetProperty("event").GetString() ?? "";
            // 5) Handle events idempotently
            switch (eventType)
            {
                case "payment.captured":
                    {
                        var payment = root.GetProperty("payload").GetProperty("payment").GetProperty("entity");
                        var paymentId = payment.GetProperty("id").GetString()!;
                        var orderId = payment.TryGetProperty("order_id", out var ord) ? ord.GetString() ?? "" : "";
                        var amount = payment.GetProperty("amount").GetInt32(); // paise
                        var currency = payment.GetProperty("currency").GetString() ?? "INR";

                        if (await _repo.PaymentAlreadyHandledAsync(paymentId))
                            return Ok(); // idempotent

                        await _repo.MarkPaymentCapturedAsync(orderId, paymentId, amount, currency);
                        break;
                    }
                case "payment.failed":
                    {
                        var payment = root.GetProperty("payload").GetProperty("payment").GetProperty("entity");
                        var paymentId = payment.GetProperty("id").GetString()!;
                        var orderId = payment.TryGetProperty("order_id", out var ord) ? ord.GetString() ?? "" : "";
                        var reason = payment.TryGetProperty("error_reason", out var rsn) ? rsn.GetString() ?? "" : "unknown";

                        if (await _repo.PaymentAlreadyHandledAsync(paymentId))
                            return Ok();

                        await _repo.MarkPaymentFailedAsync(orderId, paymentId, reason);
                        break;
                    }
                case "order.paid":
                    {
                        var order = root.GetProperty("payload").GetProperty("order").GetProperty("entity");
                        var orderId = order.GetProperty("id").GetString()!;
                        await _repo.MarkOrderPaidAsync(orderId);
                        break;
                    }
                default:
                    Console.WriteLine($"[Webhook] Ignored event: {eventType}");
                    break;
            }
            // 6) Acknowledge quickly
            return Ok();
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key ?? ""));
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = hmac.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static bool SecureEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            var diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
