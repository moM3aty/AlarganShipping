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
                dispatchOrder.DispatchDate = DateTime.Now;
                _context.Add(dispatchOrder);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }
    }
}