using Microsoft.AspNetCore.Mvc;
using WebApp.DTOs;
using WebApp.ApiClients;
using WebApp.ViewModels;
using System.Text.Json;
using Microsoft.VisualBasic;



namespace WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductApiClient _productApiClient;
        private readonly CategoriesApiClient _categoriesApiClient;
        private readonly ReviewApiClient _reviewApiClient;
        private readonly ILogger<CategoriesController> _logger;
        private const string CART_SESSION_KEY = "Cart";


        public ProductController(ProductApiClient productApiClient, CategoriesApiClient categoriesApiClient, ReviewApiClient reviewApiClient, ILogger<CategoriesController> logger)
        {
            _productApiClient = productApiClient;
            _categoriesApiClient = categoriesApiClient;
            _reviewApiClient = reviewApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? category)
        {
            try
            {
                var products = await _productApiClient.LoadProductsAsync();
                var categories = await _categoriesApiClient.LoadCategoriesAsync();
                var reviews = await _reviewApiClient.LoadReviewsAsync();

                if (!string.IsNullOrWhiteSpace(category))
                {
                    products = products.Where(p => categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name == category).ToList();
                }

                var productViewModels = new List<ProductViewModel>();

                foreach (var product in products)
                {
                    var productReview = await _reviewApiClient.LoadReviewsByProductId(product.Id);
                    double? averageRating = productReview.Any() ? productReview.Average(r => r.Rating) : null;

                    productViewModels.Add(new ProductViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        CategoryName = categories.FirstOrDefault(c => c.Id == product.CategoryId)?.Name ?? "Nema kategoriju",
                        AverageRating = averageRating
                    });

                }

                var vm = new ProductIndexViewModel
                {
                    Products = productViewModels,
                    Categories = categories.Select(c => c.Name).Distinct().ToList()
                };

                return View(vm);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get products to main page");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Product controller Index method");
                return View("Error", "An unexpected error occurred.");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productApiClient.LoadProductAsync(id);
                var category = product != null ? await _categoriesApiClient.LoadCategoryAsync(product.CategoryId) : null;

                if (product == null)
                    return NotFound();

                var vm = new DetailsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryName = category?.Name ?? "Nema kategoriju",
                    AverageRating = product.AverageRating,
                    Reviews = await _reviewApiClient.LoadReviewsByProductId(product.Id),
                };

                return View(vm);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get products details to main page");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Product controller Details method");
                return View("Error", "An unexpected error occurred.");
            }
        }

        [HttpPost("by-ids")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByIds([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0) return BadRequest("No product IDs provided.");

                var allProducts = await _productApiClient.LoadProductsAsync();
                var filteredProducts = allProducts.Where(p => ids.Contains(p.Id)).ToList();

                return Ok(filteredProducts);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get products by id");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Product controller GetProductsByIds method");
                return View("Error", "An unexpected error occurred.");
            }
        }
        [HttpPost]
        public IActionResult AddToCart(ProductCartViewModel model)
        {
            var cart = GetCartFromSession();

            var existing = cart.FirstOrDefault(x => x.Id == model.Id);
            if (existing != null)
                existing.Quantity += model.Quantity;
            else
                cart.Add(model);

            SaveCartToSession(cart);

            return RedirectToAction("Index", "Cart");
        }
        
        [HttpGet]
        public async Task<IActionResult> ProductReviews()
        {
            try
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get product reviews");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Product controller ProductReviews method");
                return View("Error", "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProductReviews(ProductReviewsViewModel model)
        {
            try
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to post product reviews");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Product controller post ProductReviews method");
                return View("Error", "An unexpected error occurred.");
            }
        }

        private List<ProductCartViewModel> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CART_SESSION_KEY);
            return !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<List<ProductCartViewModel>>(json) ?? new()
                : new();
        }

        private void SaveCartToSession(List<ProductCartViewModel> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_SESSION_KEY, json);
        }
    }
}

