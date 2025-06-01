using System.ComponentModel.DataAnnotations;
using WebApp.DTOs;

namespace WebApp.ViewModels
{
    public class DetailsViewModel
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
        public DateTime? LastReviewDate { get; set; }
        public List<ReviewDTO> Reviews { get; set; } = new();
    }
}
