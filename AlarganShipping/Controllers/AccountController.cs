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
                // 1. التحقق مما إذا كان المستخدم موظفاً / مدير
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

                // 2. التحقق مما إذا كان المستخدم عميلاً مسجلاً في البوابة
                // تم استخدام حقل Email في واجهة الدخول ليستقبل إما الايميل للموظف أو اليوزرنيم للعميل
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.PortalUsername == model.Email && c.PortalPassword == model.Password && c.IsPortalActive);

                if (customer != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()), // حفظ ID العميل
                        new Claim(ClaimTypes.Name, customer.Name),
                        new Claim(ClaimTypes.Email, customer.PortalUsername ?? ""),
                        new Claim(ClaimTypes.Role, "Customer") // إعطاء صلاحية عميل فقط
                    };

                    await SignInAsync(claims);
                    return RedirectToAction("Index", "CustomerPortal");
                }

                ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة، أو الحساب معطل.");
            }

            return View(model);
        }

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