namespace NetReach.Api.Services
{
    public class ProductService
    {
        private readonly string _basePath = AppContext.BaseDirectory;

        public List<string> GetRandomItems(int type, int quantity)
        {
            string fileName = type switch
            {
                0 => "twitterr.txt",
                1 => "instagram.txt",  // ✅ تأكد من الاسم
                2 => "proxyy.txt",
                3 => "codes.txt",
                _ => throw new ArgumentException("Invalid product type")
            };

            string filePath = Path.Combine(_basePath, "Products", fileName);
            
            // 🔍 LOG للتحقق
            Console.WriteLine($"[ProductService] Looking for file: {filePath}");
            Console.WriteLine($"[ProductService] File exists: {File.Exists(filePath)}");
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[ProductService] ❌ ERROR: File not found: {filePath}");
                return new List<string> { "PRODUCT FILE NOT FOUND - CONTACT SUPPORT" };
            }

            var allLines = File.ReadAllLines(filePath)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim().Trim('"'))  // ✅ Remove quotes
                .ToList();
                
            Console.WriteLine($"[ProductService] Found {allLines.Count} items in file");
            
            if (allLines.Count == 0)
            {
                Console.WriteLine($"[ProductService] ❌ ERROR: File is empty");
                return new List<string> { "OUT OF STOCK - CONTACT SUPPORT" };
            }

            var random = new Random();
            var selectedItems = allLines.OrderBy(x => random.Next()).Take(quantity).ToList();
            
            Console.WriteLine($"[ProductService] ✅ Returning {selectedItems.Count} items");
            
            return selectedItems;
        }

        public string GetProductName(int type)
        {
            return type switch
            {
                0 => "Twitter/X Account",
                1 => "Instagram Account",
                2 => "Premium Proxy",
                3 => "Tool License Code",
                _ => "Product"
            };
        }
    }
}
