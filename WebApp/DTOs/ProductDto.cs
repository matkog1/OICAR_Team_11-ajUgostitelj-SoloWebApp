
using System.ComponentModel.DataAnnotations;

namespace WebApp.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double? AverageRating { get; set; }
    }
}
