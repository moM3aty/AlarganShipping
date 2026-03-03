using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class DispatchOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DispatchOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dispatchOrders = await _context.DispatchOrders
                .Include(d => d.Car)
                .Include(d => d.Transporter)
                .Include(d => d.OriginLocation)
                .Include(d => d.DestinationLocation)
                .OrderByDescending(d => d.Id) // الترتيب من الأحدث للأقدم
                .ToListAsync();
            return View(dispatchOrders);
        }

        public IActionResult Create()
        {
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN");
            ViewBag.TransporterId = new SelectList(_context.Transporters, "Id", "Name");
            ViewBag.LocationId = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DispatchOrder dispatchOrder)
        {
            ModelState.Remove("Car");
            ModelState.Remove("Transporter");
            ModelState.Remove("OriginLocation");
            ModelState.Remove("DestinationLocation");

            if (ModelState.IsValid)
            {
                try
                {
                    dispatchOrder.DispatchDate = DateTime.Now;

                    // إعطاء حالة افتراضية لتجنب خطأ الـ Null في صفحة العرض
                    if (string.IsNullOrEmpty(dispatchOrder.Status))
                    {
                        dispatchOrder.Status = "Assigned";
                    }

                    _context.Add(dispatchOrder);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // التقاط أخطاء قاعدة البيانات بدلاً من الفشل الصامت
                    return Json(new { success = false, errors = new[] { "خطأ في قاعدة البيانات: " + (ex.InnerException?.Message ?? ex.Message) } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // شاشة التعديل (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dispatchOrder = await _context.DispatchOrders.FindAsync(id);
            if (dispatchOrder == null) return NotFound();

            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN", dispatchOrder.CarId);
            ViewBag.TransporterId = new SelectList(_context.Transporters, "Id", "Name", dispatchOrder.TransporterId);
            ViewBag.LocationId = new SelectList(_context.Locations, "Id", "Name");

            return View(dispatchOrder);
        }

        // ==========================================
        // حفظ التعديلات (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DispatchOrder dispatchOrder)
        {
            if (id != dispatchOrder.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Car");
            ModelState.Remove("Transporter");
            ModelState.Remove("OriginLocation");
            ModelState.Remove("DestinationLocation");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dispatchOrder);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DispatchOrderExists(dispatchOrder.Id)) return Json(new { success = false, errors = new[] { "الأمر غير موجود." } });
                    else throw;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "خطأ في قاعدة البيانات: " + (ex.InnerException?.Message ?? ex.Message) } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // حذف أمر نقل (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dispatchOrder = await _context.DispatchOrders.FindAsync(id);
            if (dispatchOrder == null) return Json(new { success = false, message = "الأمر غير موجود." });

            try
            {
                _context.DispatchOrders.Remove(dispatchOrder);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        private bool DispatchOrderExists(int id)
        {
            return _context.DispatchOrders.Any(e => e.Id == id);
        }
    }
}