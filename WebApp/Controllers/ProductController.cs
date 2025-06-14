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
        private readonly TablesApiClient _tablesApiClient;
        private readonly ILogger<CategoriesController> _logger;
        private const string CART_SESSION_KEY = "Cart";

        public ProductController(ProductApiClient productApiClient, CategoriesApiClient categoriesApiClient, ReviewApiClient reviewApiClient, TablesApiClient tablesApiClient, ILogger<CategoriesController> logger)
        {
            _productApiClient = productApiClient;
            _categoriesApiClient = categoriesApiClient;
            _reviewApiClient = reviewApiClient;
            _tablesApiClient = tablesApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? category)
        {
            try
            {
                var products = await _productApiClient.LoadProductsAsync();
                var categories = await _categoriesApiClient.LoadCategoriesAsync();
                var reviews = await _reviewApiClient.LoadReviewsAsync();
                var tables = await _tablesApiClient.LoadTablesAsync();

                if (Request.Query.ContainsKey("table")
                   && int.TryParse(Request.Query["table"], out var t))
                {
                    HttpContext.Session.SetInt32("SelectedTableId", t);
                }

                var selectedTable = HttpContext.Session.GetInt32("SelectedTableId") ?? 1;
                
                if (!string.IsNullOrWhiteSpace(category))
                {
                    products = products.Where(p => categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name == category).ToList();
                }

                var avgByProduct = reviews.GroupBy(r => r.ProductId).ToDictionary(g => g.Key,g => (double?)g.Average(r => r.Rating));


                var productViewModels = new List<ProductViewModel>();

                foreach (var product in products)
                {

                    productViewModels.Add(new ProductViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        CategoryName = categories.First(c => c.Id == product.CategoryId).Name,
                        AverageRating = avgByProduct.GetValueOrDefault(product.Id)
                    });

                }

                var vm = new ProductIndexViewModel
                {
                    Products = productViewModels,
                    Categories = categories.Select(c => c.Name).Distinct().ToList(),
                    SelectedTableId = selectedTable
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

                var reviews = await _reviewApiClient.LoadReviewsByProductId(id);

                var vm = new DetailsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryName = category?.Name ?? "Nema kategoriju",
                    Reviews = reviews,
                    AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : null,
                    LastReviewDate = reviews.Any() ? reviews.Max(r => r.ReviewDate) : null
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

