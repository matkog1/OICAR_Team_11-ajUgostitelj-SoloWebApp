namespace WebApp.ViewModels
{
    public class PaymentFormViewModel
    {
        public List<ProductCartViewModel> CartItems { get; set; } = new();
        public string Method { get; set; }
        public string CardholderName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string Cvv { get; set; }
    }
}
