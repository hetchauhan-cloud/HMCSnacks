using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HMCSnacks.Data;
using HMCSnacks.Models;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Mvc.Rendering;
using HMCSnacks.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;


namespace HMCSnacks.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        /// GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            try
            {
                ViewBag.States = await _context.States
                    .Where(s => s.IsActive)
                    .Select(s => new { s.Id, s.StateName })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error (optional): Console.WriteLine("State load error: " + ex.Message);
                ViewBag.States = new List<object>(); // Fallback to empty list
                TempData["RegisterError"] = "Unable to load states at this time. Please try again later.";
            }

            return View();
        }


        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["RegisterError"] = "Please correct the errors and try again.";
                ViewBag.States = _context.States.ToList();
                ViewBag.Cities = _context.Cities.ToList();
                return View(model);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

            if (existingUser != null)
            {
                TempData["RegisterError"] = "Username or Email already exists.";
                ViewBag.States = _context.States.ToList();
                ViewBag.Cities = _context.Cities.ToList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                Name = model.Name,
                Username = model.Username,
                Email = model.Email,
                Role = "User",
                MobileNumber = model.MobileNumber,
                StateId = model.StateId,
                CityId = model.CityId,
                Pincode = model.Pincode,

                // Map fixed address fields
                Address1 = model.Address1,
                Address2 = model.Address2,
                Address3 = model.Address3,
                Address4 = model.Address4
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            string subject = "🎉 Welcome to HMC Snacks!";
            var emailBody = _emailService.SendWelcomeEmailAsync(user.Name);
            await _emailService.SendEmailAsync(user.Email, subject, emailBody);

            TempData["RegisterSuccess"] = "Account created successfully! Please login.";
            return RedirectToAction("Login");
        }





        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    // ✅ Check if 2FA is enabled
                    if (!user.IsTwoFactorEnabled)
                    {
                        // 🚫 2FA disabled → directly login user
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("Name", user.Name ?? user.Username);
                        HttpContext.Session.SetString("UserRole", user.Role);
                        HttpContext.Session.SetInt32("UserId", user.Id);
                        HttpContext.Session.SetString("Email", user.Email);

                        // ✅ Role-based redirect
                        if (user.Role == "Admin")
                            return RedirectToAction("OrderReports", "Order");
                        else
                            return RedirectToAction("Index", "Product");
                    }

                    // ✅ 2FA enabled → proceed with OTP
                    var otp = new Random().Next(100000, 999999).ToString();
                    var expiry = DateTime.UtcNow.AddMinutes(10);

                    var userOtp = new UserOTP
                    {
                        UserId = user.Id,
                        OTP = otp,
                        ExpiryTime = expiry,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserOTPs.Add(userOtp);
                    await _context.SaveChangesAsync();

                    var emailBody = _emailService.GetLoginOtpEmailTemplate(user.Name, otp);
                    await _emailService.SendEmailAsync(user.Email, "Your Login OTP - HMC Snacks", emailBody);

                    HttpContext.Session.SetInt32("Pending2FAUserId", user.Id);

                    return RedirectToAction("Verify2FAOTP", "Account");
                }

                ModelState.AddModelError("", "Invalid login credentials.");
            }

            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Verify2FAOTP()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FAOTP(Verify2FAOTP model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetInt32("Pending2FAUserId");
            if (userId == null)
            {
                TempData["OtpError"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            var otpEntry = await _context.UserOTPs
                .Where(x => x.UserId == userId && x.IsUsed == false && x.OTP == model.OTP && x.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpEntry == null)
            {
                TempData["OtpError"] = "Invalid or expired OTP.";
                return View(model);
            }

            // Mark OTP as used
            otpEntry.IsUsed = true;
            await _context.SaveChangesAsync();

            // Proceed to log the user in
            var user = await _context.Users.FindAsync(userId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Name", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.Remove("Pending2FAUserId");

            TempData["LoginSuccess"] = "2FA verification successful.";

            return user.Role?.ToLower() == "admin"
                ? RedirectToAction("OrderReports", "Order")
                : RedirectToAction("Index", "Product");
        }


        // GET: /Account/Logout
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            // ✅ Clear all session values
            HttpContext.Session.Clear();

            TempData["LogoutSuccess"] = "You have been logged out.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email?.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
            {
                TempData["OtpError"] = "Email not found.";
                return View(model);
            }

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store in TempData or Session (safer: Session)
            HttpContext.Session.SetString("ResetEmail", user.Email);
            HttpContext.Session.SetString("ResetOtp", otp);
            HttpContext.Session.SetString("OtpExpiry", DateTime.UtcNow.AddMinutes(5).ToString());

            string subject = "Your OTP for Password Reset";
            var emailBody = _emailService.GetOtpEmailTemplate(user.Name, otp);

            await _emailService.SendEmailAsync(user.Email, subject, emailBody);

            

            // Send email (replace with actual email sender)
            Console.WriteLine($"Send OTP to {user.Email}: {otp}");

            TempData["OtpSent"] = "OTP sent to your email!";
            return RedirectToAction("VerifyOtp");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp() => View();

        [HttpPost]
        [AllowAnonymous]
        public IActionResult VerifyOtp(VerifyOtp model)
        {
            if (!ModelState.IsValid) return View(model);

            var storedOtp = HttpContext.Session.GetString("ResetOtp");
            var expiryStr = HttpContext.Session.GetString("OtpExpiry");

            if (storedOtp == model.Otp && DateTime.UtcNow <= DateTime.Parse(expiryStr))
            {
                HttpContext.Session.SetString("OtpVerified", "true");
                return RedirectToAction("ResetPassword");
            }

            TempData["OtpError"] = "Invalid or expired OTP.";
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("OtpVerified") != "true")
                return RedirectToAction("ForgotPassword");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = HttpContext.Session.GetString("ResetEmail");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                TempData["OtpError"] = "User not found.";
                return RedirectToAction("ForgotPassword");
            }

            var hasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
            await _context.SaveChangesAsync();

            // Clear session
            HttpContext.Session.Remove("ResetOtp");
            HttpContext.Session.Remove("ResetEmail");
            HttpContext.Session.Remove("OtpVerified");
            HttpContext.Session.Remove("OtpExpiry");

            TempData["PasswordUpdated"] = "Password updated successfully!";
            return RedirectToAction("Login");
        }



        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetCities(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { c.Id, c.CityName })
                .ToList();

            return Json(cities);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult UpdateProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return NotFound();

            var viewModel = new UpdateProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                StateId = user.StateId,
                CityId = user.CityId,
                Pincode = user.Pincode,
                Address1 = user.Address1,
                Address2 = user.Address2,
                Address3 = user.Address3,
                Address4 = user.Address4
            };

            ViewBag.States = new SelectList(_context.States.ToList(), "Id", "StateName", user.StateId);
            ViewBag.Cities = new SelectList(_context.Cities.Where(c => c.StateId == user.StateId).ToList(), "Id", "CityName", user.CityId);

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult UpdateProfile(UpdateProfileViewModel model)
        {
            // Force model binder to ignore removed/empty addresses
            ModelState.Remove("Address2");
            ModelState.Remove("Address3");
            ModelState.Remove("Address4");

            if (!ModelState.IsValid)
            {
                ViewBag.States = new SelectList(_context.States.ToList(), "Id", "StateName", model.StateId);
                ViewBag.Cities = new SelectList(
                    _context.Cities.Where(c => c.StateId == model.StateId).ToList(),
                    "Id", "CityName", model.CityId
                );
                return View(model);
            }

            var username = HttpContext.Session.GetString("Username");
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (existingUser == null)
                return NotFound();

            existingUser.Name = model.Name;
            existingUser.Email = model.Email;
            existingUser.MobileNumber = model.MobileNumber;
            existingUser.StateId = model.StateId;
            existingUser.CityId = model.CityId;
            existingUser.Pincode = model.Pincode;

            // Clean up blank addresses before saving
            existingUser.Address1 = model.Address1;
            existingUser.Address2 = string.IsNullOrWhiteSpace(model.Address2) ? null : model.Address2;
            existingUser.Address3 = string.IsNullOrWhiteSpace(model.Address3) ? null : model.Address3;
            existingUser.Address4 = string.IsNullOrWhiteSpace(model.Address4) ? null : model.Address4;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("OrderReports", "Order");
        }


    }
}
