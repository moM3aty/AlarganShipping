// مسار الملف: Controllers/CarsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;
using System;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // قائمة السيارات
        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.Customer) // ربط بيانات العميل
                .Include(c => c.Auction)  // ربط بيانات المزاد
                .Include(c => c.Shipment) // ربط بيانات الشحنة
                .OrderByDescending(c => c.Id)
                .ToListAsync();
            return View(cars);
        }

        // شاشة الإضافة
        public IActionResult Create()
        {
            // تعبئة القوائم المنسدلة للعملاء والمزادات
            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name");
            ViewData["AuctionId"] = new SelectList(_context.Auctions.OrderBy(a => a.Name), "Id", "Name");
            return View();
        }

        // حفظ السيارة الجديدة (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            // 💡 الحل الجذري: مسح أخطاء التحقق للقوائم والكائنات المرتبطة التي لا نرسلها من الفورم
            ModelState.Remove("Customer");
            ModelState.Remove("Auction");
            ModelState.Remove("Shipment");
            ModelState.Remove("TrackingLogs");
            ModelState.Remove("Invoices");
            ModelState.Remove("DocumentAttachments");
            ModelState.Remove("CarInspections");

            if (ModelState.IsValid)
            {
                car.StatusId = 1; // تعيين الحالة الافتراضية "تم الشراء"

                // توليد كود داخلي للسيارة إذا كان فارغاً
                if (string.IsNullOrEmpty(car.InternalCode))
                {
                    car.InternalCode = "AUTO-" + DateTime.Now.Year + "-" + new Random().Next(1000, 9999);
                }

                try
                {
                    _context.Add(car);
                    await _context.SaveChangesAsync(); // الحفظ الفعلي في قاعدة البيانات

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // التقاط أي خطأ من قاعدة البيانات وإرساله للواجهة
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, errors = new[] { ex.InnerException?.Message ?? ex.Message } });
                }
            }

            // في حالة فشل التحقق، نرسل الأخطاء للواجهة
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }

            // إعادة تعبئة القوائم في حال تم الإرسال العادي (بدون AJAX) وفشل التحقق
            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name", car.CustomerId);
            ViewData["AuctionId"] = new SelectList(_context.Auctions.OrderBy(a => a.Name), "Id", "Name", car.AuctionId);
            return View(car);
        }

        // تفاصيل السيارة والتتبع
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .Include(c => c.Shipment)
                .Include(c => c.TrackingLogs.OrderByDescending(t => t.UpdateDate)) // جلب سجلات التتبع مرتبة
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            return View(car);
        }
    }
}