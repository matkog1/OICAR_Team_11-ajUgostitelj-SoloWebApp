namespace WebApp.ViewModels
{
    public class CartIndexViewModel
    {
        public List<ProductCartViewModel> Items { get; set; } = new();
        public int SelectedTableId { get; set; }
    }
}
