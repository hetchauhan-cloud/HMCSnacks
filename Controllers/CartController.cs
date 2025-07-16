using Microsoft.AspNetCore.Mvc;
using HMCSnacks.Data;
using HMCSnacks.Models;
using System.Text.Json;

namespace HMCSnacks.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to get cart from session
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return cartJson != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJson) : new List<CartItem>();
        }

        // Helper to save cart to session
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
                return NotFound();

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(p => p.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1
                });
            }

            SaveCart(cart);
            TempData["CartMessage"] = $"{product.ProductName} added to cart!";
            return RedirectToAction("ProductList", "Product");  // Change as per your product list page
        }

        public IActionResult ViewCart()
        {
            var cart = GetCart();
            return View(cart);
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("ViewCart");
        }
    }
}
