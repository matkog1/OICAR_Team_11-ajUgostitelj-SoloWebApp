using System.ComponentModel.DataAnnotations;

namespace WebApp.DTOs
{
    public class StockCheckDto
    {
        public int Id { get; set; }
        public DateTime CheckDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int RecordedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int Difference => ActualQuantity - RecordedQuantity;
    }
}
