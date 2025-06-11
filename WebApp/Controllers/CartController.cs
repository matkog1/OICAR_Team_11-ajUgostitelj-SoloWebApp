using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class CartController : Controller
    {
        private const string CART_SESSION_KEY = "Cart";
        public CartController()
        {
            
        }
        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCartFromSession(); 
            var selectedTableId = HttpContext.Session.GetInt32("SelectedTableId") ?? 2;
            var vm = new CartIndexViewModel
            {
                Items = cart,
                SelectedTableId = selectedTableId
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = GetCartFromSession();

            var updatedCart = cart.Where(x => x.Id != id).ToList();

            SaveCartToSession(updatedCart);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout([FromBody] List<ProductCartViewModel> items)
        {
            if (items == null || !items.Any())
                return BadRequest("Cart is empty.");

            SaveCartToSession(items);

            return Json(new { redirectUrl = Url.Action("Process", "Payment") });
        }


        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        private List<ProductCartViewModel> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CART_SESSION_KEY);
            return !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<List<ProductCartViewModel>>(json) ?? new List<ProductCartViewModel>()
                : new List<ProductCartViewModel>();
        }

        private void SaveCartToSession(List<ProductCartViewModel> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_SESSION_KEY, json);
        }

    }
}