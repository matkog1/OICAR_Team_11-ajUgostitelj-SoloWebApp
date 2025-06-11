using Microsoft.AspNetCore.Mvc;
using WebApp.ApiClients;
using WebApp.ViewModels;
using WebApp.DTOs;
using System.Text.Json;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;

namespace WebApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentApiClient _paymentApiClient;
        private readonly OrderApiClient _orderApiClient;
        private readonly ILogger<CategoriesController> _logger;
        private readonly PayPalHttpClient _paypalClient;
        private const string CART_SESSION_KEY = "Cart";

        public PaymentController(PaymentApiClient paymentApiClient, OrderApiClient orderApiClient, ILogger<CategoriesController> logger, PayPalHttpClient paypalClient)
        {
            _paymentApiClient = paymentApiClient;
            _orderApiClient = orderApiClient;
            _logger = logger;
            _paypalClient = paypalClient;
        }

        [HttpGet]
        public IActionResult Process(int tableId)
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
                return RedirectToAction("Error", "Product");

            var vm = new PaymentFormViewModel
            {
                TableId = tableId,
                CartItems = cart,
                TotalAmount = (int)cart.Sum(x => x.Price * x.Quantity)
            };
            return View(vm); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(PaymentFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
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
                    TableId = vm.TableId,
                    Status = (int)OrderStatus.Paid,
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
                    Method = vm.Method,
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

        [HttpPost]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] PayPalDTO dto)
        {
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    ReferenceId = dto.Reference.ToString(),
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = dto.Currency,
                        Value        = dto.Amount.ToString("F2")
                    }
                }
            }
            });

            var response = await _paypalClient.Execute(request);
            var order = response.Result<Order>();
            return Json(new { id = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CapturePayPalOrder([FromBody] CaptureOrderDTO dto)
        {
            var request = new OrdersCaptureRequest(dto.OrderID);
            request.RequestBody(new OrderActionRequest());
            var response = await _paypalClient.Execute(request);
            var result = response.Result<PayPalDTO>();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> FinalizePayPal(string orderID, int tableId)
        {
            // 1) dohvat ordera
            var captureRequest = new OrdersCaptureRequest(orderID);
            captureRequest.RequestBody(new OrderActionRequest());
            var captureResponse = await _paypalClient.Execute(captureRequest);
            var payPalOrder = captureResponse.Result<Order>();

            // 2) radimo order
            var cart = GetCartFromSession();
            var orderDto = new OrderDto
            {
                OrderDate = DateTime.UtcNow,
                TableId = tableId,
                Status = (int)OrderStatus.Paid,
                TotalAmount = cart.Sum(x => x.Price * x.Quantity)
            };
            var createdOrder = await _orderApiClient.CreateOrder(orderDto);

            // dodavanje itemsa
            var items = cart.Select(c => new OrderItemDto
            {
                OrderId = createdOrder.Id,
                ProductId = c.Id,
                ProductName = c.Name,
                UnitPrice = c.Price,
                Quantity = c.Quantity
            }).ToList();
            await _orderApiClient.AddOrderItemsToOrder(items);

            // 4) Rradimo payment
            var paymentDto = new PaymentDto
            {
                Amount = createdOrder.TotalAmount,
                Method = "PayPal",
                PaymentDate = DateTime.UtcNow,
                OrderId = createdOrder.Id
            };
            await _paymentApiClient.CreatePaymentAsync(paymentDto);


            HttpContext.Session.Remove(CART_SESSION_KEY);
            return RedirectToAction("Success", new
            {
                id = createdOrder.Id,
                orderStatus = createdOrder.Status,
                table = createdOrder.TableId
            });
        }
    }
}