using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة المواقع
        public async Task<IActionResult> Index()
        {
            return View(await _context.Locations.ToListAsync());
        }

        // شاشة الإضافة (GET)
        public IActionResult Create()
        {
            return View();
        }

        // حفظ الموقع الجديد (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Location location)
        {
            ModelState.Remove("OriginDispatchOrders");
            ModelState.Remove("DestinationDispatchOrders");

            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // شاشة التعديل (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            return View(location);
        }

        // حفظ التعديلات (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Location location)
        {
            if (id != location.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("OriginDispatchOrders");
            ModelState.Remove("DestinationDispatchOrders");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id)) return Json(new { success = false, errors = new[] { "الموقع غير موجود." } });
                    else throw;
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // حذف موقع (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return Json(new { success = false, message = "الموقع غير موجود." });
            }

            // حماية البيانات: منع حذف موقع مستخدم في الشحنات أو أوامر النقل
            bool hasShipments = await _context.Shipments.AnyAsync(s => s.LoadingPortId == id || s.DischargePortId == id);
            bool hasDispatchOrders = await _context.DispatchOrders.AnyAsync(d => d.OriginLocationId == id || d.DestinationLocationId == id);

            if (hasShipments || hasDispatchOrders)
            {
                return Json(new { success = false, message = "لا يمكن حذف هذا الموقع لارتباطه بشحنات أو أوامر نقل مسجلة." });
            }

            try
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }

        // ==============================================================
        // دالة الإضافة السريعة (تم حل مشكلة قيم الـ Null الإجبارية هنا)
        // ==============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuick(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "الاسم مطلوب" });

            try
            {
                var location = new Location
                {
                    Name = name,
                    // تمرير قيم افتراضية لتجاوز شروط قاعدة البيانات
                    Address = "غير محدد",
                    Country = "غير محدد",
                    StateOrProvince = "غير محدد",
                    LocationType = "مستودع / ساحة",
                    ContactPerson = "-",
                    ContactPhone = "-"
                };

                _context.Add(location);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = location.Id, name = location.Name });
            }
            catch (Exception ex)
            {
                // إرجاع رسالة الخطأ الدقيقة لتسهيل تتبعها في حال حدوث مشكلة أخرى
                string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "حدث خطأ أثناء الحفظ: " + errorMsg });
            }
        }
    }
}