using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم لوحة التحكم الرئيسية (Dashboard)
    [Authorize] // حماية الصفحة لعدم دخول غير المسجلين
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // جلب الإحصائيات لعرضها في البطاقات العلوية
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.ActiveShipments = await _context.Shipments.CountAsync(s => s.Status != "Cleared" && s.Status != "Delivered");
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();

            // حساب إجمالي المبالغ المعلقة للعملاء
            ViewBag.PendingAmounts = await _context.Customers.SumAsync(c => c.TotalBalance);

            // جلب أحدث 5 سيارات تمت إضافتها للنظام لعرضها في جدول سريع
            var recentCars = await _context.Cars
                .Include(c => c.Customer)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();

            return View(recentCars);
        }

        // دالة لتغيير لغة الموقع وحفظها في Cookie
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}