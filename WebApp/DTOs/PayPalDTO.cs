namespace WebApp.DTOs
{
    public class PayPalDTO
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public int Reference { get; set; }  
    }
}
