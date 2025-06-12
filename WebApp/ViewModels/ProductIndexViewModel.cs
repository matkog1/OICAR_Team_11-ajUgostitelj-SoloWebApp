using System.ComponentModel.DataAnnotations;
using WebApp.DTOs;

namespace WebApp.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string? ImageUrl { get; set; }
        public double? AverageRating { get; set; }
    }

    public class ProductIndexViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<TableDto> Tables { get; set; } = new();

        public  int  SelectedTableId { get; set; }
    }
}
