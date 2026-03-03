using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class CarInspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarInspectionsController(ApplicationDbContext context)
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
        public async Task<IActionResult> Create(CarInspection inspection)
        {
            ModelState.Remove("Car");

            if (ModelState.IsValid)
            {
                inspection.InspectionDate = DateTime.Now;
                _context.Add(inspection);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }
    }
}