// مسار الملف: Controllers/ShipmentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    [Authorize]
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
            // جلب الموانئ فقط لتعبئة القوائم المنسدلة
            var ports = _context.Locations.Where(l => l.LocationType == "Port").OrderBy(l => l.Name).ToList();
            ViewData["LoadingPortId"] = new SelectList(ports, "Id", "Name");
            ViewData["DischargePortId"] = new SelectList(ports, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipment shipment)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(shipment.Status))
                {
                    shipment.Status = "Pending";
                }

                _context.Add(shipment);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }

            var ports = _context.Locations.Where(l => l.LocationType == "Port").OrderBy(l => l.Name).ToList();
            ViewData["LoadingPortId"] = new SelectList(ports, "Id", "Name", shipment.LoadingPortId);
            ViewData["DischargePortId"] = new SelectList(ports, "Id", "Name", shipment.DischargePortId);
            return View(shipment);
        }

        // إضافة دالة لعرض تفاصيل الشحنة (التي كانت مفقودة وتسبب خطأ عند الضغط على "المحتويات")
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var shipment = await _context.Shipments
                .Include(s => s.LoadingPort)
                .Include(s => s.DischargePort)
                .Include(s => s.Cars) // جلب السيارات داخل هذه الشحنة
                    .ThenInclude(c => c.Customer) // جلب أصحاب السيارات
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shipment == null) return NotFound();

            return View(shipment);
        }
    }
}