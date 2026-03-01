using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم تقارير فحص السيارات (لمطابقة حالة السيارة عند الاستلام)
    [Authorize]
    public class CarInspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarInspectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // شاشة إضافة تقرير فحص جديد
        public IActionResult Create(int? carId)
        {
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", carId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarInspection inspection)
        {
            if (ModelState.IsValid)
            {
                inspection.InspectionDate = DateTime.Now;
                _context.Add(inspection);
                await _context.SaveChangesAsync();

                // العودة إلى تفاصيل السيارة لمشاهدة التقرير
                return RedirectToAction("Details", "Cars", new { id = inspection.CarId });
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", inspection.CarId);
            return View(inspection);
        }
    }
}