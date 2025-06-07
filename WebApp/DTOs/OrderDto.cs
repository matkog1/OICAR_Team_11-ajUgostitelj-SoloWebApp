namespace WebApp.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; } 
        public DateTime OrderDate { get; set; }
        public int Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int TableId { get; set; }
    }
}
