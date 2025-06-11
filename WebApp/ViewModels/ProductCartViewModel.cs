namespace WebApp.ViewModels
{
    public class ProductCartViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int TableId { get; set; }
    }

}
