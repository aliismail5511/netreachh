using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetReach.Api.Services
{
    public class CryptomusService
    {
        private readonly string _merchantId = "edd0baa0-0a28-4eb4-97ca-08f7bbd450d6";
        private readonly string _apiKey = "F5nHD0gpZxjLBzoe42PuCRPAOSgrErp2fiRAdgJJ9FkB4B0JbS366YVUTPNh7qgiRadmz2kRjF4Rlcfy2Vg8KGLpMrKfSkCNFGC7L4e2Fc2EPnuGeliagkxsNo47Iv1O";
        private readonly HttpClient _httpClient;

        public CryptomusService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(string? url, string? uuid, string? error)> CreateInvoiceAsync(decimal amount, string orderId, string callbackUrl, string returnUrl)
        {
            // Create payload with all required fields
            var payload = new
            {
                amount = amount.ToString("F2"),
                currency = "USD",
                order_id = orderId,
                url_callback = callbackUrl,
                url_return = returnUrl,
                is_payment_multiple = false,
                lifetime = 3600, // 1 hour
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            
            // Generate signature BEFORE base64 encoding
            var signature = GenerateSignature(jsonPayload);

            // Log for debugging
            Console.WriteLine($"[Cryptomus] Creating invoice for order: {orderId}");
            Console.WriteLine($"[Cryptomus] Payload: {jsonPayload}");
            Console.WriteLine($"[Cryptomus] Signature: {signature}");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.cryptomus.com/v1/payment");
            request.Headers.Add("merchant", _merchantId);
            request.Headers.Add("sign", signature);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[Cryptomus] Response Status: {response.StatusCode}");
                Console.WriteLine($"[Cryptomus] Response Body: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Cryptomus] Error: {content}");
                    return (null, null, content);
                }

                dynamic result = JsonConvert.DeserializeObject(content)!;
                
                // Check if result exists
                if (result?.result == null)
                {
                    Console.WriteLine("[Cryptomus] No result in response");
                    return (null, null, "No result in response");
                }

                string? url = result.result.url;
                string? uuid = result.result.uuid;

                Console.WriteLine($"[Cryptomus] Invoice created successfully: {uuid}");
                
                return (url, uuid, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cryptomus] Exception: {ex.Message}");
                Console.WriteLine($"[Cryptomus] Stack Trace: {ex.StackTrace}");
                return (null, null, ex.Message);
            }
        }

        private string GenerateSignature(string jsonPayload)
        {
            // Step 1: Base64 encode the JSON payload
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload));
            
            // Step 2: Concatenate base64 + API key
            var signatureInput = base64Payload + _apiKey;
            
            // Step 3: MD5 hash
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signatureInput));
            
            // Step 4: Convert to lowercase hex string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public bool VerifyWebhookSignature(string requestBody, string receivedSign)
        {
            try
            {
                // Parse the JSON
                var jsonObject = JObject.Parse(requestBody);
                
                // Remove the 'sign' field if it exists
                jsonObject.Remove("sign");
                
                // Sort the keys alphabetically (Cryptomus requirement)
                var sortedJson = new JObject(jsonObject.Properties().OrderBy(p => p.Name));
                
                // Serialize back to JSON (compact, no spaces)
                var jsonData = JsonConvert.SerializeObject(sortedJson, Formatting.None);
                
                // Calculate expected signature
                var expectedSign = GenerateSignature(jsonData);
                
                Console.WriteLine($"[Cryptomus Webhook] Cleaned JSON: {jsonData}");
                Console.WriteLine($"[Cryptomus Webhook] Received sign: {receivedSign}");
                Console.WriteLine($"[Cryptomus Webhook] Expected sign: {expectedSign}");
                
                return expectedSign == receivedSign;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cryptomus Webhook] Verification error: {ex.Message}");
                return false;
            }
        }
    }
}