using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WebApp.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<CategoriesController> _logger;

        public ErrorController(ILogger<CategoriesController> logger)
        {
            _logger = logger;
        }

        [Route("Error")]
        public IActionResult Index()
        {
            try
            {
                ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                return View();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get errors");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Error controller index method");
                return View("Error", "An unexpected error occurred.");
            }
        }
    }
}
