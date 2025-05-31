using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.ApiClients;

namespace WebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly CategoriesApiClient _categoriesApiClient;
        private readonly ILogger<CategoriesController> _logger;


        public CategoriesController(CategoriesApiClient categoriesApiClient, ILogger<CategoriesController> logger)
        {
            _categoriesApiClient = categoriesApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoriesApiClient.LoadCategoriesAsync();
                return View(categories);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch categories.");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CategoriesController.Index");
                return View("Error", "An unexpected error occurred.");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _categoriesApiClient.LoadCategoryAsync(id);
                if (category == null) return NotFound();

                return View(category);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get categories details.");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Categories controller details method");
                return View("Error", "An unexpected error occurred.");
            }
        }
    }
}
