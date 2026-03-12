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
            ModelState.Remove("LoadingPort"); ModelState.Remove("DischargePort");
            ModelState.Remove("Cars"); ModelState.Remove("Documents");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shipment shipment)
        {
            if (id != shipment.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("LoadingPort"); ModelState.Remove("DischargePort");
            ModelState.Remove("Cars"); ModelState.Remove("Documents");

            if (ModelState.IsValid)
            {
                try
                {
                    // 💡 الترقية الذكية (Smart Upgrade): 
                    // جلب الشحنة بالسيارات المرتبطة بها لمقارنة تغير الحالة
                    var oldShipment = await _context.Shipments.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    var linkedCars = await _context.Cars.Where(c => c.ShipmentId == id).ToListAsync();

                    _context.Update(shipment);

                    // إذا تغيرت حالة الشحنة إلى مبحرة أو واصلة، نقوم بتحديث السيارات وإشعار العملاء
                    if (oldShipment != null && oldShipment.Status != shipment.Status)
                    {
                        foreach (var car in linkedCars)
                        {
                            if (shipment.Status == "Sailed")
                            {
                                car.StatusId = 6; // أبحرت
                                _context.TrackingLogs.Add(new TrackingLog { CarId = car.Id, Title = "أبحرت السفينة", Description = $"تم إبحار الحاوية رقم {shipment.ContainerNumber}", Location = "في البحر", ProgressPercentage = 83, UpdateDate = DateTime.Now });
                                _context.Notifications.Add(new Notification { CustomerId = car.CustomerId, Title = "إبحار شحنة", Message = $"سيارتك {car.Make} أبحرت وهي في الطريق إليك.", Type = "TrackingUpdate" });
                            }
                            else if (shipment.Status == "Arrived")
                            {
                                car.StatusId = 7; // وصلت
                                _context.TrackingLogs.Add(new TrackingLog { CarId = car.Id, Title = "وصول للميناء الوجهة", Description = $"السفينة وصلت للميناء بنجاح.", Location = "الميناء", ProgressPercentage = 100, UpdateDate = DateTime.Now });
                                _context.Notifications.Add(new Notification { CustomerId = car.CustomerId, Title = "وصول السيارة!", Message = $"سيارتك {car.Make} وصلت إلى الميناء المخصص. يرجى الترتيب للتخليص.", Type = "TrackingUpdate" });
                            }
                            _context.Update(car);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "حدث خطأ: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shipment = await _context.Shipments.Include(s => s.Cars).FirstOrDefaultAsync(s => s.Id == id);
            if (shipment == null) return Json(new { success = false, message = "الشحنة غير موجودة." });

            if (shipment.Cars != null && shipment.Cars.Any())
            {
                return Json(new { success = false, message = "لا يمكن حذف هذه الشحنة لأنها تحتوي على سيارات. قم بفك الارتباط أولاً." });
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

        [HttpGet]
        public async Task<IActionResult> GetAvailableCars()
        {
            var availableCars = await _context.Cars
                .Where(c => c.ShipmentId == null && (c.StatusId == 1 || c.StatusId == 2 || c.StatusId == 4))
                .Select(c => new { id = c.Id, make = c.Make, model = c.Model, vin = c.VIN })
                .ToListAsync();

            return Json(availableCars);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkCar(int shipmentId, int carId)
        {
            var shipment = await _context.Shipments.Include(s => s.Cars).FirstOrDefaultAsync(s => s.Id == shipmentId);
            var car = await _context.Cars.FindAsync(carId);

            if (shipment == null || car == null) return Json(new { success = false, message = "البيانات غير صحيحة." });
            if (shipment.Cars.Count >= 4) return Json(new { success = false, message = "الحاوية ممتلئة (الحد الأقصى 4 سيارات)." });

            // 💡 تحديث تلقائي للحالة وإرسال إشعار
            car.ShipmentId = shipmentId;
            car.StatusId = 5; // جاري التحميل

            _context.TrackingLogs.Add(new TrackingLog
            {
                CarId = car.Id,
                Title = "تم التحميل في الحاوية",
                Description = $"تم ربط السيارة بالحاوية رقم {shipment.ContainerNumber} وجاهزة للإقلاع.",
                Location = "الميناء",
                ProgressPercentage = 66,
                UpdateDate = DateTime.Now
            });

            _context.Notifications.Add(new Notification
            {
                CustomerId = car.CustomerId,
                Title = "تحديث الشحن",
                Message = $"تم تحميل سيارتك {car.Make} {car.Model} بنجاح داخل الحاوية وتنتظر الإقلاع.",
                Type = "TrackingUpdate"
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkCar(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) return Json(new { success = false, message = "السيارة غير موجودة." });

            car.ShipmentId = null;
            car.StatusId = 2; // تعود للمستودع

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}