using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using System.Text.Json;

namespace AlarganShipping.Controllers
{
    // نموذج مؤقت للأقسام (يمكن نقله لقاعدة البيانات لاحقاً)
    public class DepartmentInfo
    {
        public required string Title { get; set; }
        public required string Icon { get; set; }
        public required string Description { get; set; }
        public required List<string> Phones { get; set; }
    }


    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContactController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            // 1. جلب بيانات التواصل من ملف الإعدادات
            string settingsPath = Path.Combine(_env.WebRootPath, "settings.json");
            SettingsViewModel settings = new SettingsViewModel();
            if (System.IO.File.Exists(settingsPath))
            {
                var json = System.IO.File.ReadAllText(settingsPath);
                settings = JsonSerializer.Deserialize<SettingsViewModel>(json) ?? new SettingsViewModel();
            }
            ViewBag.Settings = settings;

            // 2. جلب الفروع ديناميكياً من قاعدة البيانات (الأماكن التي نوعها Branch أو تقع في عمان)
            var branches = await _context.Locations
                .Where(l => l.LocationType == "Branch" || l.Country.Contains("Oman") || l.Country.Contains("عمان") || l.Country.Contains("UAE"))
                .ToListAsync();
            ViewBag.Branches = branches;

            // 3. جلب الموظفين الذين لديهم صلاحية الظهور في صفحة اتصل بنا
            var contactStaff = await _context.Users
                .Where(u => u.ShowOnContactPage && u.IsActive)
                .ToListAsync();

            return View(contactStaff);
        }
    }
}