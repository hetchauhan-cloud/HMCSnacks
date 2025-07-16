using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using HMCSnacks.Data;
using HMCSnacks.Models;
using HMCSnacks.Models.ViewModels;
using System.Data.Common;

namespace HMCSnacks.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Shows all available products and current cart items
        public async Task<IActionResult> Index()
        {
            // ✅ Fetch only active products
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            // ✅ Wrap each product in a ViewModel
            var viewModels = products.Select(p => new ProductDisplayViewModel
            {
                Product = p
            }).ToList();

            // ✅ Retrieve cart from session
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            ViewBag.Cart = cart;
            ViewBag.CartCount = cart.Sum(c => c.Quantity);

            // ✅ Return ViewModel to View (not raw Product list)
            return View(viewModels);
        }



        // ✅ Shows cart content
        public IActionResult ViewCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            return View(cartItems);
        }

        // ✅ POST: Add to cart with quantity validation
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["CartMessage"] = "❌ Please select a valid quantity.";
                return RedirectToAction("Index");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            var currentQtyInCart = existingItem?.Quantity ?? 0;
            var totalRequestedQty = currentQtyInCart + quantity;

            // 🔐 Validate total does not exceed stock
            if (totalRequestedQty > product.StockQuantity)
            {
                var remainingStock = product.StockQuantity - currentQtyInCart;
                TempData["CartMessage"] = $"⚠️ Cannot add {quantity}. Only {remainingStock} more left in stock.";
                return RedirectToAction("Index");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = totalRequestedQty;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));

            TempData["CartMessage"] = $"✅ {product.ProductName} (Qty: {quantity}) added to cart.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
                TempData["CartMessage"] = "Item removed successfully.";
            }

            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["CartMessage"] = "Cart cleared successfully.";
            return RedirectToAction("ViewCart");
        }

        // ✅ GET: Product List with Pagination using SP
        public async Task<IActionResult> ProductList(int page = 1, int pageSize = 5)
        {
            var products = new List<Product>();
            int offset = (page - 1) * pageSize;

            // Filter IsActive=true when calculating total count
            var totalCount = await _context.Products.CountAsync(p => p.IsActive);

            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM get_all_products(@offset, @limit)";

            var offsetParam = cmd.CreateParameter();
            offsetParam.ParameterName = "offset";
            offsetParam.Value = offset;
            cmd.Parameters.Add(offsetParam);

            var limitParam = cmd.CreateParameter();
            limitParam.ParameterName = "limit";
            limitParam.Value = pageSize;
            cmd.Parameters.Add(limitParam);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    StockQuantity = reader.GetInt32(4),
                    IsActive = reader.GetBoolean(5),
                    IsBestSeller = reader.GetBoolean(6)
                });
            }

            // ✅ Convert to ViewModel
            var productVMs = products.Select(p => new ProductDisplayViewModel
            {
                Product = p
            }).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(productVMs); // ✅ Pass ViewModel to view
        }



        // ✅ GET: Edit Product by ID (fills AddProduct form)
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            Product product = null;

            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM get_product_by_id(@p_id)";

            var idParam = cmd.CreateParameter();
            idParam.ParameterName = "p_id";
            idParam.Value = id;
            cmd.Parameters.Add(idParam);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                product = new Product
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Category = reader.IsDBNull(4) ? null : reader.GetString(4),
                    StockQuantity = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    WeightOrSize = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PackType = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ImagePath = reader.IsDBNull(8) ? null : reader.GetString(8),
                    IsBestSeller = !reader.IsDBNull(9) && reader.GetBoolean(9)
                };
            }

            if (product == null)
                return NotFound();

            return View("AddProduct", product); // ✅ Reuse AddProduct.cshtml for editing
        }


        // GET: Show Add Product Form
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View(new Product()); // Empty product for new entry
        }

        // POST: Add or Update Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product)
        {
            // ❌ Remove ImagePath validation since it's set dynamically
            ModelState.Remove(nameof(product.ImagePath));
            ModelState.Remove(nameof(product.ProductImage));

            // ❌ Check model validation after removal
            if (!ModelState.IsValid)
                return View(product);

            // 📁 Handle image upload
            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                Directory.CreateDirectory(uploadDir); // Ensure directory exists

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ProductImage.FileName);
                string fullPath = Path.Combine(uploadDir, uniqueFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await product.ProductImage.CopyToAsync(fileStream);
                }

                product.ImagePath = "/images/products/" + uniqueFileName;
            }
            else if (product.Id == 0)
            {
                // ❗ Require image only on creation
                ModelState.AddModelError("ImagePath", "Please upload a product image.");
                return View(product);
            }

            // 🔄 Database logic
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            if (product.Id == 0)
            {
                // ➕ Insert new product
                cmd.CommandText = "CALL sp_add_product(@p_name, @p_price, @p_desc, @p_category, @p_stock, @p_weight_size, @p_pack_type, @p_image, @p_best_seller)";
            }
            else
            {
                // ✏️ Update existing product
                cmd.CommandText = "CALL sp_update_product(@p_id, @p_name, @p_price, @p_desc, @p_category, @p_stock, @p_weight_size, @p_pack_type, @p_image, @p_best_seller)";
                cmd.Parameters.Add(CreateParam(cmd, "p_id", product.Id));
            }

            // ✅ Add parameters
            cmd.Parameters.Add(CreateParam(cmd, "p_name", product.ProductName));
            cmd.Parameters.Add(CreateParam(cmd, "p_price", product.Price));
            cmd.Parameters.Add(CreateParam(cmd, "p_desc", product.Description ?? string.Empty));
            cmd.Parameters.Add(CreateParam(cmd, "p_category", product.Category ?? string.Empty));
            cmd.Parameters.Add(CreateParam(cmd, "p_stock", product.StockQuantity));
            cmd.Parameters.Add(CreateParam(cmd, "p_weight_size", product.WeightOrSize ?? string.Empty));
            cmd.Parameters.Add(CreateParam(cmd, "p_pack_type", product.PackType ?? string.Empty));
            cmd.Parameters.Add(CreateParam(cmd, "p_image", product.ImagePath ?? string.Empty));
            cmd.Parameters.Add(CreateParam(cmd, "p_best_seller", product.IsBestSeller));

            await cmd.ExecuteNonQueryAsync();

            TempData["Success"] = product.Id == 0 ? "Product added successfully!" : "Product updated successfully!";
            return RedirectToAction("ProductList");
        }



        // 🔧 Helper method to create parameters
        private DbParameter CreateParam(DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            return param;
        }


        [HttpPost]
        public async Task<IActionResult> SoftDeleteProduct(int id)
        {
            try
            {
                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE ""Products"" SET ""IsActive"" = FALSE WHERE ""Id"" = @id";

                var idParam = cmd.CreateParameter();
                idParam.ParameterName = "id";
                idParam.Value = id;
                cmd.Parameters.Add(idParam);

                var affectedRows = await cmd.ExecuteNonQueryAsync();

                if (affectedRows > 0)
                {
                    TempData["Success"] = "Product deleted successfully.";
                }
                else
                {
                    TempData["Success"] = "Product not found or already deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Success"] = "An error occurred while deleting the product.";
                // Optionally log the exception: _logger.LogError(ex, "Soft delete failed.");
            }

            return RedirectToAction("ProductList");
        }

        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            // Fetch order items
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;

                    // Optional: Set inactive if stock becomes 0
                    if (product.StockQuantity <= 0)
                    {
                        product.StockQuantity = 0;
                        product.IsActive = false;
                    }
                }
            }

            // Save changes after all updates
            await _context.SaveChangesAsync();

            // Update order status
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = "Approved";
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Order Approved and Stock Updated!";
            return RedirectToAction("OrderList");
        }








    }
}
