using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using HMCSnacks.Data;
using HMCSnacks.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMCSnacks.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔁 Helper method to retrieve the cart from session
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cartItems = GetCart();

            if (!cartItems.Any())
            {
                TempData["CartEmpty"] = "Your cart is empty.";
                return RedirectToAction("ViewCart", "Product");
            }

            // ✅ Calculate total
            decimal total = cartItems.Sum(c => c.Price * c.Quantity);
            ViewBag.Total = total;

            // ✅ Get user addresses
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            var addresses = new List<string>();
            if (!string.IsNullOrWhiteSpace(user?.Address1)) addresses.Add(user.Address1);
            if (!string.IsNullOrWhiteSpace(user?.Address2)) addresses.Add(user.Address2);
            if (!string.IsNullOrWhiteSpace(user?.Address3)) addresses.Add(user.Address3);
            if (!string.IsNullOrWhiteSpace(user?.Address4)) addresses.Add(user.Address4);

            ViewBag.Addresses = new SelectList(addresses);

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string SelectedAddress)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["CartEmpty"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // ✅ SAFEGUARD: Check stock again
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || item.Quantity > product.StockQuantity)
                {
                    TempData["CartMessage"] = $"❌ Not enough stock for {item.ProductName}. Please adjust quantity.";
                    return RedirectToAction("ViewCart", "Product");
                }
            }

            var name = HttpContext.Session.GetString("Name");
            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");

            var order = new Order
            {
                Name = name,
                Username = username,
                Email = email,
                DeliveryAddress = SelectedAddress, 
                Items = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    Price = c.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            // ✅ Reduce stock
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.StockQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("ThankYou", new { id = order.Id });
        }




        // ✅ GET: /Checkout/ThankYou
        public IActionResult ThankYou(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OrderReport(int page = 1, int pageSize = 5)
        {
            var username = HttpContext.Session.GetString("Username");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true"; // Assuming you store admin info like this

            var query = _context.Orders
                .Include(o => o.Items)
                .AsQueryable();

            if (!isAdmin && !string.IsNullOrEmpty(username))
            {
                query = query.Where(o => o.Username == username);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.IsAdmin = isAdmin;

            return View(orders);
        }

    }
}
