using Microsoft.AspNetCore.Mvc;
using WebApp.ApiClients;

namespace WebApp.Controllers
{
    public class StatusController : Controller
    {
        private readonly OrderApiClient _orderApiClient; 
        private readonly ILogger<CategoriesController> _logger;

        public StatusController(OrderApiClient orderApiClient, ILogger<CategoriesController> logger)
        {
            _orderApiClient = orderApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var lastOrderId = HttpContext.Session.GetInt32("LastOrderId");
                if (lastOrderId == null)
                    return RedirectToAction("Index", "Product");

                var order = await _orderApiClient.GetOrderById(lastOrderId.Value);
                if (order == null)
                    return NotFound("Order not found.");

                return View(order);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get status");
                return View("Error", "Unable to connect to the API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Status controller Index method");
                return View("Error", "An unexpected error occurred.");
            }
        }
    }
}
