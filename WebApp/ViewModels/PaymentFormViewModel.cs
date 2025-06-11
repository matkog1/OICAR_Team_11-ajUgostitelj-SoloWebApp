namespace WebApp.ViewModels
{
    public class PaymentFormViewModel
    {
        public List<ProductCartViewModel> CartItems { get; set; } = new();
        public string Method { get; set; }

        public int TableId { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
