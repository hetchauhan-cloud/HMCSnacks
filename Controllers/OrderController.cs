using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HMCSnacks.Data;
using HMCSnacks.Models;

namespace HMCSnacks.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> OrderReports(OrderReportFilterViewModel filterModel, int page = 1, int pageSize = 5)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            IQueryable<Order> query = _context.Orders.Include(o => o.Items).Where(o => !o.IsOrderDeleted);

            // 🔐 Restrict non-admin users
            if (role?.ToLower() != "admin")
            {
                query = query.Where(o => o.Username == username);
            }

            // 🔍 Apply Filters
            if (!string.IsNullOrWhiteSpace(filterModel.Status))
            {
                query = query.Where(o => o.OrderStatus.ToLower() == filterModel.Status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filterModel.CustomerName) && role?.ToLower() == "admin")
            {
                query = query.Where(o => o.Name.ToLower().Contains(filterModel.CustomerName.ToLower()));
            }

            if (filterModel.FromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(filterModel.FromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(o => o.OrderDate >= from);
            }

            if (filterModel.ToDate.HasValue)
            {
                var to = DateTime.SpecifyKind(filterModel.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(o => o.OrderDate <= to);
            }

            // 📄 Pagination
            int totalOrders = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            filterModel.Orders = orders;

            ViewBag.IsAdmin = role?.ToLower() == "admin";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            return View("~/Views/Order/OrderReports.cshtml", filterModel);
        }

        [HttpGet]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("UserRole");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null || order.OrderStatus.ToLower() != "pending")
            {
                return NotFound();
            }

            // ✅ If customer, only cancel their own order
            if (role?.ToLower() != "admin")
            {
                if (order.Username != username)
                    return Unauthorized();

                order.OrderStatus = "Cancelled";
                order.IsCancelledByCustomer = true;
            }
            else
            {
                // ✅ Admin cancelling
                order.OrderStatus = "Cancelled";
                order.IsCancelledByAdmin = true;
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Order has been cancelled.";
            return RedirectToAction("OrderReports");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role?.ToLower() != "admin")
                return Unauthorized();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null || !(order.OrderStatus.ToLower() == "rejected" || order.OrderStatus.ToLower() == "completed"))
            {
                return NotFound();
            }

            // ✅ Ensure OrderDate is UTC (to avoid Npgsql error)
            if (order.OrderDate.Kind == DateTimeKind.Unspecified)
            {
                order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
            }

            // ✅ Soft delete
            order.IsOrderDeleted = true;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order has been marked as deleted.";
            return RedirectToAction("OrderReports");
        }


        public async Task<IActionResult> ViewOrderDetails(int id)
        {
             var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status, string adminComment)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.OrderStatus = status;
            order.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order has been {status.ToLower()}!";
            return RedirectToAction("OrderReports");
        }


    }
}
