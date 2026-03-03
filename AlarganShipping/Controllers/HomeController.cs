using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization; // تمت الإضافة

namespace AlarganShipping.Controllers
{
    [Authorize] // هذا السطر يجبر النظام على عدم فتح لوحة التحكم إلا للمسجلين
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.ActiveShipments = await _context.Shipments.CountAsync(s => s.Status != "Arrived" && s.Status != "Cleared");

            decimal totalSales = await _context.Invoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
            decimal totalPaid = await _context.PaymentReceipts.SumAsync(p => (decimal?)p.Amount) ?? 0;

            ViewBag.TotalSales = totalSales;
            ViewBag.PendingAmounts = totalSales - totalPaid;

            var recentCars = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();

            return View(recentCars);
        }

        [HttpPost]
        [AllowAnonymous] // السماح بتغيير اللغة حتى في شاشة الدخول
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl);
        }
    }
}