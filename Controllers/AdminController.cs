using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMCSnacks.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // This will be your admin landing page

        public IActionResult OrderDetails()
        {
            // Later: fetch order data from DB and pass to the view
            return View();
        }
    }
}
