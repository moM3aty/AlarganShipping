using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

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

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. جلب الفروع ديناميكياً من قاعدة البيانات
            // (سنجلب المواقع والموانئ الموجودة في عمان كمثال للفروع)
            var branches = await _context.Locations
                .Where(l => l.Country.Contains("Oman") || l.Country.Contains("عمان") || l.LocationType == "Branch")
                .ToListAsync();

            // 2. إعداد بيانات الأقسام بشكل ديناميكي
            var departments = new List<DepartmentInfo>
            {
                new DepartmentInfo
                {
                    Title = "قسم المزادات",
                    Icon = "fa-gavel",
                    Description = "للدعم الفني في حسابات كوبارت و IAAI والمزايدة المباشرة.",
                    Phones = new List<string> { "96505777", "96202777" }
                },
                new DepartmentInfo
                {
                    Title = "قسم خدمة العملاء",
                    Icon = "fa-headset",
                    Description = "لمتابعة حالة الشحنات، الدعم الفني، وتقديم الاقتراحات.",
                    Phones = new List<string> { "77521020" }
                },
                new DepartmentInfo
                {
                    Title = "قسم المبيعات",
                    Icon = "fa-tags",
                    Description = "للاستفسارات التجارية، عروض الأسعار، والتعاقدات الجديدة.",
                    Phones = new List<string> { "94203663", "77263111" }
                }
            };

            ViewBag.Departments = departments;
            ViewBag.Branches = branches;

            return View();
        }
    }
}