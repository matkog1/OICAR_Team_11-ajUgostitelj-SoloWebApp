using Microsoft.AspNetCore.Mvc;
using WebApp.ApiClients;
using WebApp.ViewModels;
using WebApp.DTOs;
using System.Text.Json;

namespace WebApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentApiClient _paymentApiClient;
        private readonly OrderApiClient _orderApiClient;
        private readonly ILogger<CategoriesController> _logger;
        private const string CART_SESSION_KEY = "Cart";

        public PaymentController(PaymentApiClient paymentApiClient, OrderApiClient orderApiClient, ILogger<CategoriesController> logger)
        {
            _paymentApiClient = paymentApiClient;
            _orderApiClient = orderApiClient;
            _logger = logger;
        }

        [HttpGet, HttpPost]
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            TempData.Keep("Cart");
            return View(cart);
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> Process([FromForm] PaymentFormViewModel viewmodel)
        {
            try
            {
                var cart = GetCartFromSession();
                if (!cart.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                var orderDto = new OrderDto
                {
                    OrderDate = DateTime.UtcNow,
                    TableId = 2,
                    Status = "Paid",
                    TotalAmount = cart.Sum(x => x.Price * x.Quantity)
                };

                var createdOrder = await _orderApiClient.CreateOrder(orderDto);
                if (createdOrder == null)
                {
                    _logger.LogWarning("Failed to create order");
                    return View("Checkout");
                }

                var orderItems = cart.Select(c => new OrderItemDto
                {
                    OrderId = createdOrder.Id,
                    ProductId = c.Id,
                    ProductName = c.Name,
                    UnitPrice = c.Price,
                    Quantity = c.Quantity
                }).ToList();

                var added = await _orderApiClient.AddOrderItemsToOrder(orderItems);

                var paymentDto = new PaymentDto
                {
                    Amount = createdOrder.TotalAmount,
                    Method = "cash",
                    PaymentDate = DateTime.UtcNow,
                    OrderId = createdOrder.Id
                };

                var payment = await _paymentApiClient.CreatePaymentAsync(paymentDto);
                HttpContext.Session.SetInt32("LastOrderId", createdOrder.Id);

                return RedirectToAction("Success", new { id = createdOrder.Id, orderStatus = createdOrder.Status, table = orderDto.TableId });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API communication fail during payment.");
                TempData["Error"] = "There was a problem processing your payment.";
                return View("Checkout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PaymentController.Process.");
                return View("Error", "An unexpected error occurred.");
            }
        }

        public IActionResult Success(int id, string orderStatus, int table)
        {
            ViewBag.OrderId = id;
            ViewBag.OrderStatus = orderStatus;
            ViewBag.TableNumber = table;
            return View("Success");
        }
        private List<ProductCartViewModel> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CART_SESSION_KEY);
            return !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<List<ProductCartViewModel>>(json) ?? new()
                : new();
        }
    }
}