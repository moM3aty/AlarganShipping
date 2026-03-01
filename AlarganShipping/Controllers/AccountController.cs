using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using AlarganShipping.Models.ViewModels;

namespace AlarganShipping.Controllers
{
    // متحكم إدارة تسجيل الدخول والصلاحيات
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
            // إذا كان المستخدم مسجل دخوله بالفعل، يتم تحويله للوحة التحكم
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // البحث عن المستخدم في قاعدة البيانات
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password && u.IsActive);

                // إضافة وهمية لتسهيل الدخول لأول مرة قبل إضافة مستخدمين فعليين
                if (user == null && model.Email == "admin@alargan.com" && model.Password == "123456")
                {
                    user = new User { FullName = "مدير النظام", Email = "admin@alargan.com", Role = "Admin" };
                }

                if (user != null)
                {
                    // إنشاء الصلاحيات (Claims)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة، أو الحساب غير نشط.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            // تسجيل الخروج ومسح ملفات الارتباط (Cookies)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}