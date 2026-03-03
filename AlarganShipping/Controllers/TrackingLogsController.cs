using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class TrackingLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrackingLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create(int? carId)
        {
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN", carId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrackingLog log)
        {
            ModelState.Remove("Car");

            if (ModelState.IsValid)
            {
                log.UpdateDate = DateTime.Now;
                _context.Add(log);

                // تحديث حالة السيارة أوتوماتيكياً بناءً على نسبة الإنجاز
                var car = await _context.Cars.FindAsync(log.CarId);
                if (car != null)
                {
                    if (log.ProgressPercentage >= 100) car.StatusId = 4; // واصلة الميناء
                    else if (log.ProgressPercentage >= 66) car.StatusId = 3; // أبحرت
                    else if (log.ProgressPercentage >= 33) car.StatusId = 2; // في المستودع
                    else car.StatusId = 1; // تم الشراء

                    _context.Update(car);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }
    }
}