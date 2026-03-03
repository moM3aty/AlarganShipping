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
    }
}