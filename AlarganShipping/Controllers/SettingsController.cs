using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using System.Text.Json;
using System.IO;

namespace AlarganShipping.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _settingsFilePath;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _settingsFilePath = Path.Combine(_env.WebRootPath, "settings.json");
        }

        public async Task<IActionResult> Index()
        {
            var model = GetCurrentSettings();

            // جلب بعض الإحصائيات لإضافتها في تقرير الـ PDF
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.TotalInvoices = await _context.Invoices.CountAsync();
            ViewBag.TotalBalance = await _context.Customers.SumAsync(c => (decimal?)c.TotalBalance) ?? 0;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSettings(SettingsViewModel model)
        {
            try
            {
                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_settingsFilePath, json);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "خطأ في حفظ الإعدادات." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WipeData()
        {
            try
            {
                _context.TrackingLogs.RemoveRange(_context.TrackingLogs);
                _context.Invoices.RemoveRange(_context.Invoices);
                _context.PaymentReceipts.RemoveRange(_context.PaymentReceipts);
                _context.Cars.RemoveRange(_context.Cars);
                _context.Customers.RemoveRange(_context.Customers);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
        }

        private SettingsViewModel GetCurrentSettings()
        {
            if (System.IO.File.Exists(_settingsFilePath))
            {
                var json = System.IO.File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<SettingsViewModel>(json) ?? new SettingsViewModel();
            }
            return new SettingsViewModel();
        }
    }

    // تم إضافة الحقول الجديدة الخاصة بنماذج الفواتير هنا
    public class SettingsViewModel
    {
        public decimal UsdExchangeRate { get; set; } = 0.385m;
        public decimal AedExchangeRate { get; set; } = 0.105m;
        public decimal CustomsPercentage { get; set; } = 5;
        public decimal VatPercentage { get; set; } = 5;
        public decimal DefaultCommission { get; set; } = 150;
        public string InquiryPhoneNumber { get; set; } = "96890000000";

        // إعدادات الفواتير والطباعة (Invoice Settings)
        public string InvoiceTemplate { get; set; } = "Modern"; // Classic, Modern, Simple
        public string InvoiceHeaderColor { get; set; } = "#1e293b"; // لون الترويسة الافتراضي (كحلي)
        public string VatNumber { get; set; } = "00016800820";
        public string CrNumber { get; set; } = "0001425763";
        public string InvoiceFooterText { get; set; } = "البضاعة المباعة لا ترد ولا تستبدل إلا حسب الشروط والأحكام. شكراً لثقتكم بنا.";
    }
}