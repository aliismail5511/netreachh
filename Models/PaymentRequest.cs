namespace NetReach.Api.Models
{
    public class PaymentRequest
    {
        public decimal Price { get; set; }
        public string PayCurrency { get; set; } = "usd";
        public string Email { get; set; } = string.Empty;
        public int Type { get; set; }
        public int Quantity { get; set; }
    }
}
