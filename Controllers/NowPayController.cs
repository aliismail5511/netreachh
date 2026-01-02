using Microsoft.AspNetCore.Mvc;
using NetReach.Api.Models;
using NetReach.Api.Services;

namespace NetReach.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NowPayController : ControllerBase
    {
        private readonly CryptomusService _cryptomusService;

        public NowPayController(CryptomusService cryptomusService)
        {
            _cryptomusService = cryptomusService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] PaymentRequest request)
        {
            if (request == null || request.Price <= 0)
            {
                return BadRequest(new { error = "Invalid request", message = "Price must be greater than 0" });
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { error = "Invalid request", message = "Email is required" });
            }

            var orderId = $"{Guid.NewGuid()}|{request.Email}|{request.Type}|{request.Quantity}";
            var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/Payment/Webhook";
            var returnUrl = "https://net-reach.vercel.app/success";

            Console.WriteLine($"[Order] Creating payment for: {request.Email}");
            Console.WriteLine($"[Order] Order ID: {orderId}");
            Console.WriteLine($"[Order] Amount: ${request.Price}");

            var (url, uuid, error) = await _cryptomusService.CreateInvoiceAsync(
                request.Price, 
                orderId, 
                callbackUrl,
                returnUrl
            );

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(uuid))
            {
                Console.WriteLine($"[Order] Failed to create payment: {error}");
                return StatusCode(500, new 
                { 
                    error = "Failed to create payment",
                    details = error,
                    orderId = orderId
                });
            }

            Console.WriteLine($"[Order] Payment created successfully");
            Console.WriteLine($"[Order] UUID: {uuid}");
            Console.WriteLine($"[Order] URL: {url}");

            return Ok(new 
            { 
                success = true,
                url = url,
                uuid = uuid,
                orderId = orderId
            });
        }
    }
}