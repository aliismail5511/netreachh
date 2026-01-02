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

        [HttpPost("Webhook")]
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

                if (status != "paid" && status != "paid_over")
                {
                    Console.WriteLine("[Webhook] Payment not completed");
                    return Ok(new { status = "received", message = $"Status: {status}" });
                }

                Console.WriteLine("[Webhook] Payment CONFIRMED - Processing order");

                var parts = orderId.Split('|');
                if (parts.Length != 4)
                {
                    Console.WriteLine("[Webhook] Invalid order ID format");
                    return BadRequest("Invalid order ID format");
                }

                string email = parts[1];
                int type = int.Parse(parts[2]);
                int quantity = int.Parse(parts[3]);

                Console.WriteLine($"[Webhook] Email: {email}");
                Console.WriteLine($"[Webhook] Type: {type}");
                Console.WriteLine($"[Webhook] Quantity: {quantity}");

                var items = _productService.GetRandomItems(type, quantity);

                // ✅ حماية ضد الإيميل الفاضي
                if (items == null || items.Count == 0)
                {
                    Console.WriteLine("[Webhook] ERROR: No items found, email NOT sent");

                    return BadRequest(new
                    {
                        error = "Out of stock",
                        message = "No items available for this product"
                    });
                }

                var productName = _productService.GetProductName(type);

                Console.WriteLine($"[Webhook] Retrieved {items.Count} items");

                await _emailService.SendProductEmailAsync(email, productName, items);

                Console.WriteLine($"[Webhook] Email sent successfully to {email}");
                Console.WriteLine("==============================================");

                return Ok(new
                {
                    status = "success",
                    message = "Order processed successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Webhook] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[Webhook] Stack Trace: {ex.StackTrace}");
                Console.WriteLine("==============================================");

                return StatusCode(500, new
                {
                    error = "Internal server error",
                    details = ex.Message
                });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                ok = true,
                message = "Payment webhook endpoint is working",
                time = DateTime.UtcNow
            });
        }
        [HttpPost("test-webhook")]
public async Task<IActionResult> TestWebhook([FromBody] TestWebhookRequest request)
{
    Console.WriteLine("=== TEST WEBHOOK ===");
    
    var orderId = $"{Guid.NewGuid()}|{request.Email}|{request.Type}|{request.Quantity}";
    
    // Simulate webhook data
    var webhookData = new
    {
        status = "paid",
        order_id = orderId,
        uuid = Guid.NewGuid().ToString()
    };
    
    // Process order
    var items = _productService.GetRandomItems(request.Type, request.Quantity);
    var productName = _productService.GetProductName(request.Type);
    
    await _emailService.SendProductEmailAsync(request.Email, productName, items);
    
    Console.WriteLine($"✅ Test email sent to {request.Email}");
    
    return Ok(new { success = true, orderId, items });
}

// Test request model
public class TestWebhookRequest
{
    public string Email { get; set; } = "";
    public int Type { get; set; }
    public int Quantity { get; set; }
}
    }
}
