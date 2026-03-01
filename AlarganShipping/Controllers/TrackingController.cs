using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using System.Threading.Tasks;

namespace AlarganShipping.Controllers
{
    // المتحكم المسؤول عن بوابة التتبع العامة
    public class TrackingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض صفحة البحث الرئيسية (البوابة)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // معالجة عملية البحث عن السيارة برقم الشاصي
        [HttpPost]
        public async Task<IActionResult> Search(string vin)
        {
            if (string.IsNullOrEmpty(vin))
            {
                ViewBag.Error = "يرجى إدخال رقم الشاصي بشكل صحيح.";
                return View("Index");
            }

            // البحث عن السيارة وتضمين سجلات التتبع والعميل والمزاد
            var car = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.TrackingLogs)
                .Include(c => c.Shipment)
                .FirstOrDefaultAsync(c => c.VIN == vin || c.InternalCode == vin);

            if (car == null)
            {
                ViewBag.Error = "عذراً، لم يتم العثور على سيارة مطابقة لهذا الرقم.";
                return View("Index");
            }

            // التوجه لصفحة عرض تفاصيل التتبع للسيارة التي تم العثور عليها
            return View("Result", car);
        }
    }
}