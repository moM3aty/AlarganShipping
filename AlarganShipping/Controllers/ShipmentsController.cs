using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShipmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var shipments = await _context.Shipments
                .Include(s => s.LoadingPort)
                .Include(s => s.DischargePort)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            return View(shipments);
        }

        public IActionResult Create()
        {
            var ports = _context.Locations.Where(l => l.LocationType == "Port").ToList();
            ViewBag.LoadingPortId = new SelectList(ports, "Id", "Name");
            ViewBag.DischargePortId = new SelectList(ports, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipment shipment)
        {
            ModelState.Remove("LoadingPort");
            ModelState.Remove("DischargePort");
            ModelState.Remove("Cars");
            ModelState.Remove("Documents");

            if (ModelState.IsValid)
            {
                _context.Add(shipment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var shipment = await _context.Shipments
                .Include(s => s.LoadingPort)
                .Include(s => s.DischargePort)
                .Include(s => s.Cars).ThenInclude(c => c.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shipment == null) return NotFound();
            return View(shipment);
        }

        // ==========================================
        // شاشة التعديل (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null) return NotFound();

            var ports = _context.Locations.Where(l => l.LocationType == "Port").ToList();
            ViewBag.LoadingPortId = new SelectList(ports, "Id", "Name", shipment.LoadingPortId);
            ViewBag.DischargePortId = new SelectList(ports, "Id", "Name", shipment.DischargePortId);

            return View(shipment);
        }

        // ==========================================
        // حفظ التعديلات (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shipment shipment)
        {
            if (id != shipment.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("LoadingPort");
            ModelState.Remove("DischargePort");
            ModelState.Remove("Cars");
            ModelState.Remove("Documents");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shipment);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Shipments.Any(e => e.Id == shipment.Id)) return Json(new { success = false, errors = new[] { "الشحنة غير موجودة." } });
                    else throw;
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // حذف الشحنة والحاوية (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shipment = await _context.Shipments.Include(s => s.Cars).FirstOrDefaultAsync(s => s.Id == id);
            if (shipment == null) return Json(new { success = false, message = "الشحنة غير موجودة." });

            // حماية البيانات: منع حذف الشحنة إذا كان بها سيارات
            if (shipment.Cars != null && shipment.Cars.Any())
            {
                return Json(new { success = false, message = "لا يمكن حذف هذه الشحنة لأنها تحتوي على سيارات. قم بإزالة السيارات منها أولاً." });
            }

            try
            {
                _context.Shipments.Remove(shipment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        // ==========================================
        // دوال ربط السيارات بالحاوية (AJAX)
        // ==========================================

        // 1. جلب السيارات المتاحة في المستودع (غير مرتبطة بشحنة)
        [HttpGet]
        public async Task<IActionResult> GetAvailableCars()
        {
            // جلب السيارات التي ليس لها ShipmentId وحالتها (تم الشراء = 1 أو في المستودع = 2)
            var availableCars = await _context.Cars
                .Where(c => c.ShipmentId == null && (c.StatusId == 1 || c.StatusId == 2))
                .Select(c => new { id = c.Id, make = c.Make, model = c.Model, vin = c.VIN })
                .ToListAsync();

            return Json(availableCars);
        }

        // 2. ربط السيارة بالشحنة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkCar(int shipmentId, int carId)
        {
            var shipment = await _context.Shipments.Include(s => s.Cars).FirstOrDefaultAsync(s => s.Id == shipmentId);
            var car = await _context.Cars.FindAsync(carId);

            if (shipment == null || car == null)
                return Json(new { success = false, message = "البيانات غير صحيحة." });

            if (shipment.Cars.Count >= 4)
                return Json(new { success = false, message = "الحاوية ممتلئة بالكامل (الحد الأقصى 4 سيارات)." });

            // ربط السيارة وتغيير حالتها إلى "في الشحن" (StatusId = 3)
            car.ShipmentId = shipmentId;
            car.StatusId = 3;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 3. فك ارتباط السيارة عن الشحنة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkCar(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) return Json(new { success = false, message = "السيارة غير موجودة." });

            // فك الارتباط وإعادة الحالة إلى "في المستودع" (StatusId = 2)
            car.ShipmentId = null;
            car.StatusId = 2;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}