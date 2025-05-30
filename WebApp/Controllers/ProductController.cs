using Microsoft.AspNetCore.Mvc;
using WebApp.DTOs;
using WebApp.ApiClients;
using WebApp.ViewModels;
using System.Text.Json;



namespace WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductApiClient _productApiClient;
        private readonly CategoriesApiClient _categoriesApiClient;
        private readonly ReviewApiClient _reviewApiClient;

        public ProductController(ProductApiClient productApiClient, CategoriesApiClient categoriesApiClient, ReviewApiClient reviewApiClient)
        {
            _productApiClient = productApiClient;
            _categoriesApiClient = categoriesApiClient;
            _reviewApiClient = reviewApiClient;
        }

        public async Task<IActionResult> Index(string? category)
        {
            var products = await _productApiClient.LoadProductsAsync();
            var categories = await _categoriesApiClient.LoadCategoriesAsync();

            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name == category).ToList();
            }

            var vm = new ProductIndexViewModel
            {
                Products = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryName = categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Nema kategoriju",
                    AverageRating = p.AverageRating
                }).ToList(),
                Categories = categories.Select(c => c.Name).Distinct().ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productApiClient.LoadProductAsync(id);
            var category = product != null ? await _categoriesApiClient.LoadCategoryAsync(product.CategoryId) : null;

            if (product == null)
                return NotFound();

            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryName = category?.Name ?? "Nema kategoriju",
                AverageRating = product.AverageRating
            };

            return View(vm);
        }

        [HttpPost("by-ids")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByIds([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return BadRequest("No product IDs provided.");

            var allProducts = await _productApiClient.LoadProductsAsync();
            var filteredProducts = allProducts.Where(p => ids.Contains(p.Id)).ToList();

            return Ok(filteredProducts);
        }
        [HttpPost]
        public IActionResult AddToCart(ProductCartViewModel model)
        {
            var cart = TempData.ContainsKey("Cart")
                ? JsonSerializer.Deserialize<List<ProductCartViewModel>>(TempData["Cart"]!.ToString()!)!
                : new List<ProductCartViewModel>();

            var existing = cart.FirstOrDefault(x => x.Id == model.Id);
            if (existing != null)
                existing.Quantity += model.Quantity;
            else
                cart.Add(model);

            TempData["Cart"] = JsonSerializer.Serialize(cart);
            TempData.Keep("Cart");

            return RedirectToAction("Index", "Cart");
        }
        
        [HttpGet]
        public async Task<IActionResult> ProductReviews()
        {
            var products = await _productApiClient.LoadProductsAsync();
            var reviews = await _reviewApiClient.LoadReviewsAsync();

            var vm = new ProductReviewsViewModel
            {
                Products = products,
                Reviews = reviews
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProductReviews(ProductReviewsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = await _productApiClient.LoadProductsAsync();
                model.Reviews = await _reviewApiClient.LoadReviewsAsync();
                return View(model);
            }

            var selectedProduct = await _productApiClient.LoadProductAsync(model.NewReview.ProductId);
            if (selectedProduct == null)
            {
                ModelState.AddModelError("", "Selected product not found");
                model.Products = await _productApiClient.LoadProductsAsync();
                model.Reviews = await _reviewApiClient.LoadReviewsAsync();
                return View(model);
            }

            model.NewReview.ProductName = selectedProduct.Name;
            model.NewReview.ReviewDate = DateTime.UtcNow;

            await _reviewApiClient.CreateReviewAsync(model.NewReview);

            return RedirectToAction("ProductReviews");
        }
    }
}

