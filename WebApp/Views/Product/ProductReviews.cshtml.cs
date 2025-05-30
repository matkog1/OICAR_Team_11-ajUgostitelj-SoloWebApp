using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.ApiClients;
using WebApp.DTOs;

namespace WebApp.Pages.Product
{
    public class ReviewModel : PageModel
    {
        private readonly ProductApiClient _productApiClient;
        private readonly ReviewApiClient _reviewApiClient;

        public ReviewModel(ProductApiClient productApiClient, ReviewApiClient reviewApiClient)
        {
            _productApiClient = productApiClient;
            _reviewApiClient = reviewApiClient;
        }

        [BindProperty]
        public ReviewDTO NewReview { get; set; } = new();

        public List<ReviewDTO> Reviews { get; set; } = new();
        public List<WebApp.DTOs.ProductDto> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var selectedProduct = await _productApiClient.LoadProductAsync(NewReview.ProductId);
            if (selectedProduct == null)
            {
                ModelState.AddModelError("", "Selected product not found");
                await LoadDataAsync();
                return Page();
            }

            NewReview.ProductName = selectedProduct.Name;
            NewReview.ReviewDate = DateTime.UtcNow;

            await _reviewApiClient.CreateReviewAsync(NewReview);

            return RedirectToPage(); 
        }

        private async Task LoadDataAsync()
        {
            Products = await _productApiClient.LoadProductsAsync();
            Reviews = await _reviewApiClient.LoadReviewsAsync();
        }
    }
}
