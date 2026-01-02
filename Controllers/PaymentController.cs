using Microsoft.AspNetCore.Mvc;
using NetReach.Api.Services;
using Newtonsoft.Json;

namespace NetReach.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly EmailService _emailService;
        private readonly CryptomusService _cryptomusService;

        public PaymentController(
            ProductService productService,
            EmailService emailService,
            CryptomusService cryptomusService)
        {
            _productService = productService;
            _emailService = emailService;
            _cryptomusService = cryptomusService;
        }

        // 🔥 Cryptomus Webhook (PRODUCTION)
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                Console.WriteLine("==============================================");
                Console.WriteLine($"[Webhook] Received at: {DateTime.UtcNow}");
                Console.WriteLine($"[Webhook] Body: {body}");
                Console.WriteLine("==============================================");

                // Signature (optional – حسب إعدادك)
                var receivedSign = Request.Headers["sign"].FirstOrDefault();

                bool isTestMode = string.IsNullOrEmpty(receivedSign);
                if (isTestMode)
                {
                    Console.WriteLine("[Webhook] TEST MODE - Signature check skipped");
                }

                dynamic data = JsonConvert.DeserializeObject(body)!;

                string status = data.status;
                string orderId = data.order_id;
                string uuid = data.uuid;

                Console.WriteLine($"[Webhook] Order ID: {orderId}");
                Console.WriteLine($"[Webhook] Status: {status}");
                Console.WriteLine($"[Webhook] UUID: {uuid}");

                // ❌ Ignore unpaid orders
                if (status != "paid" && status != "paid_over")
                {
                    Console.WriteLine("[Webhook] Payment not completed");
                    return Ok(new { status = "ignored", paymentStatus = status });
                }

                Console.WriteLine("[Webhook] Payment CONFIRMED");

                // orderId format: any|email|type|quantity
                var parts = orderId.Split('|');
                if (parts.Length != 4)
                {
                    Console.WriteLine("[Webhook] ❌ Invalid order ID format");
                    return BadRequest("Invalid order ID format");
                }

                string email = parts[1];
                int type = int.Parse(parts[2]);
                int quantity = int.Parse(parts[3]);

                Console.WriteLine($"[Webhook] Email: {email}");
                Console.WriteLine($"[Webhook] Type: {type}");
                Console.WriteLine($"[Webhook] Quantity: {quantity}");

                // Get products
                var items = _productService.GetRandomItems(type, quantity);

                if (items == null || items.Count == 0)
                {
                    Console.WriteLine("[Webhook] ❌ OUT OF STOCK – Email not sent");

                    return BadRequest(new
                    {
                        error = "out_of_stock",
                        message = "No items available for this product"
                    });
                }

                var productName = _productService.GetProductName(type);

                Console.WriteLine($"[Webhook] Sending {items.Count} items");

                await _emailService.SendProductEmailAsync(email, productName, items);

                Console.WriteLine($"[Webhook] ✅ Email sent to {email}");
                Console.WriteLine("==============================================");

                return Ok(new
                {
                    status = "success",
                    message = "Order processed and email sent"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Webhook] ❌ EXCEPTION: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("==============================================");

                return StatusCode(500, new
                {
                    error = "internal_server_error",
                    message = ex.Message
                });
            }
        }

        // ✅ Simple health check
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "ok",
                service = "payment",
                time = DateTime.UtcNow
            });
        }
    }
}
