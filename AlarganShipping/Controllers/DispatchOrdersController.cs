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
                .OrderByDescending(d => d.Id)
                .ToListAsync();
            return View(dispatchOrders);
        }

        public IActionResult Create(int? carId)
        {
            // تم تغيير الأسماء لتجنب التعارض مع الموديل
            ViewBag.CarsList = new SelectList(_context.Cars, "Id", "VIN", carId);
            ViewBag.TransportersList = new SelectList(_context.Transporters, "Id", "Name");
            ViewBag.LocationsList = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DispatchOrder dispatchOrder)
        {
            ModelState.Remove("Car"); ModelState.Remove("Transporter");
            ModelState.Remove("OriginLocation"); ModelState.Remove("DestinationLocation");

            if (ModelState.IsValid)
            {
                try
                {
                    dispatchOrder.DispatchDate = DateTime.Now;
                    if (string.IsNullOrEmpty(dispatchOrder.Status)) dispatchOrder.Status = "Assigned";

                    if (string.IsNullOrEmpty(dispatchOrder.OrderNumber))
                        dispatchOrder.OrderNumber = "DIS-" + DateTime.Now.ToString("yyMMdd") + new Random().Next(100, 999);

                    _context.Add(dispatchOrder);

                    // 💡 أتمتة: تحديث حالة السيارة لسحب داخلي
                    var car = await _context.Cars.FindAsync(dispatchOrder.CarId);
                    if (car != null)
                    {
                        car.StatusId = 3; // النقل الداخلي
                        _context.TrackingLogs.Add(new TrackingLog { CarId = car.Id, Title = "أمر نقل داخلي (Dispatch)", Description = "تم تكليف السائق بسحب السيارة ونقلها.", Location = "أمريكا", ProgressPercentage = 33, UpdateDate = DateTime.Now });
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "خطأ في قاعدة البيانات: " + (ex.InnerException?.Message ?? ex.Message) } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dispatchOrder = await _context.DispatchOrders.FindAsync(id);
            if (dispatchOrder == null) return NotFound();

            // تم تغيير الأسماء لتجنب التعارض
            ViewBag.CarsList = new SelectList(_context.Cars, "Id", "VIN", dispatchOrder.CarId);
            ViewBag.TransportersList = new SelectList(_context.Transporters, "Id", "Name", dispatchOrder.TransporterId);
            ViewBag.LocationsList = new SelectList(_context.Locations, "Id", "Name");

            return View(dispatchOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DispatchOrder dispatchOrder)
        {
            if (id != dispatchOrder.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Car"); ModelState.Remove("Transporter");
            ModelState.Remove("OriginLocation"); ModelState.Remove("DestinationLocation");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dispatchOrder);

                    // 💡 أتمتة: إذا تم التسليم (Delivered)، نقل السيارة للمستودع
                    if (dispatchOrder.Status == "Delivered")
                    {
                        var car = await _context.Cars.FindAsync(dispatchOrder.CarId);
                        var dest = await _context.Locations.FindAsync(dispatchOrder.DestinationLocationId);
                        if (car != null && dest != null)
                        {
                            car.StatusId = 4; // وصول المستودع
                            car.CurrentLocation = dest.Name;

                            _context.TrackingLogs.Add(new TrackingLog { CarId = car.Id, Title = "السيارة في المستودع", Description = $"تم تسليم السيارة بنجاح في {dest.Name}.", Location = dest.Name, ProgressPercentage = 50, UpdateDate = DateTime.Now });
                            _context.Notifications.Add(new Notification { CustomerId = car.CustomerId, Title = "وصول المستودع", Message = $"سيارتك {car.Make} وصلت بأمان لمستودع التجميع في {dest.Name}.", Type = "TrackingUpdate" });
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "خطأ: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

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
    }
}