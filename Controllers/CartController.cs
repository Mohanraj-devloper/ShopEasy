using Microsoft.AspNetCore.Mvc;
using ShopEasy.Data;
using ShopEasy.Models;
using System.Text.Json;

namespace ShopEasy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString("Cart");
            return json == null
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json)!;
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }

        public IActionResult Index()
        {
            return View(GetCart());
        }

        public IActionResult Add(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item == null)
                cart.Add(new CartItem { ProductId = product.Id, Name = product.Name, Price = product.Price, Quantity = 1 });
            else
                item.Quantity++;

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            if (!GetCart().Any()) return RedirectToAction("Index");
            return View(new Order());
        }

        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            order.OrderDate = DateTime.Now;
            order.TotalAmount = cart.Sum(c => c.Total);
            order.Items = cart.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.Name,
                Price = c.Price,
                Quantity = c.Quantity
            }).ToList();

            foreach (var c in cart)
            {
                var product = _context.Products.Find(c.ProductId);
                if (product != null) product.Stock -= c.Quantity;
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Success", new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}