using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AlarganShipping.Models;
using AlarganShipping.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AlarganShipping.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. تسجيل الدخول (Login)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                    return RedirectToAction("Index", "CustomerPortal");
                else
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // التحقق من الموظفين والإدارة
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password && u.IsActive);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    await SignInAsync(claims);
                    return RedirectToAction("Index", "Home");
                }

                // التحقق من العملاء عبر بوابة العملاء
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.PortalUsername == model.Email && c.PortalPassword == model.Password && c.IsPortalActive);

                if (customer != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                        new Claim(ClaimTypes.Name, customer.Name),
                        new Claim(ClaimTypes.Email, customer.PortalUsername ?? ""),
                        new Claim(ClaimTypes.Role, "Customer")
                    };

                    await SignInAsync(claims);
                    return RedirectToAction("Index", "CustomerPortal");
                }

                ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة، أو الحساب معطل.");
            }

            return View(model);
        }

        // ==========================================
        // 2. تسجيل حساب عميل جديد (Register)
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string FullName, string Phone, string Email, string Password)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Phone) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "يرجى تعبئة جميع الحقول المطلوبة.");
                return View();
            }

            // التأكد من عدم تكرار الإيميل في النظام
            if (await _context.Customers.AnyAsync(c => c.PortalUsername == Email || c.Email == Email))
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني مستخدم بالفعل، يرجى تسجيل الدخول.");
                return View();
            }

            try
            {
                // إنشاء ملف العميل المالي والتقني
                var newCustomer = new Customer
                {
                    Name = FullName,
                    Phone = Phone,
                    Email = Email,
                    CustomerCode = "CUST-" + new Random().Next(10000, 99999),
                    PortalUsername = Email,
                    PortalPassword = Password,
                    IsPortalActive = false, // الحساب معطل حتى يوافق الأدمن
                    Address = "",
                    CivilId = "",
                    TotalBalance = 0,
                    TotalPaid = 0
                };

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إنشاء حسابك بنجاح! بانتظار موافقة الإدارة لتفعيل حسابك.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء التسجيل، يرجى المحاولة مرة أخرى.");
                return View();
            }
        }

        // ==========================================
        // 3. تسجيل الخروج والمصادقة
        // ==========================================
        private async Task SignInAsync(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}