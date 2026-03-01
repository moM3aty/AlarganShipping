using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم إضافة سجلات التتبع للسيارات
    [Authorize]
    public class TrackingLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrackingLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // شاشة إضافة تحديث لسيارة معينة (يتم تمرير رقم السيارة)
        public IActionResult Create(int? carId)
        {
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", carId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrackingLog trackingLog)
        {
            if (ModelState.IsValid)
            {
                trackingLog.UpdateDate = DateTime.Now;
                _context.Add(trackingLog);

                // تحديث حالة السيارة تلقائياً بناءً على نسبة الإنجاز
                var car = await _context.Cars.FindAsync(trackingLog.CarId);
                if (car != null)
                {
                    if (trackingLog.ProgressPercentage >= 100) car.StatusId = 4; // وصلت
                    else if (trackingLog.ProgressPercentage >= 66) car.StatusId = 3; // أبحرت
                    else if (trackingLog.ProgressPercentage >= 33) car.StatusId = 2; // في المستودع

                    _context.Update(car);
                }

                await _context.SaveChangesAsync();

                // العودة إلى شاشة تفاصيل السيارة بعد إضافة التحديث
                return RedirectToAction("Details", "Cars", new { id = trackingLog.CarId });
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", trackingLog.CarId);
            return View(trackingLog);
        }
    }
}