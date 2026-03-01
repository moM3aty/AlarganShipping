using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم إدارة الحاويات والشحنات البحرية
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
            var shipments = _context.Shipments
                .Include(s => s.LoadingPort)
                .Include(s => s.DischargePort);
            return View(await shipments.ToListAsync());
        }

        public IActionResult Create()
        {
            // جلب الموانئ فقط لتعيينها كميناء تحميل وميناء وصول
            var ports = _context.Locations.Where(l => l.LocationType == "Port").ToList();
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
                shipment.Status = "Pending"; // حالة الشحنة مبدئياً "معلقة"
                _context.Add(shipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var ports = _context.Locations.Where(l => l.LocationType == "Port").ToList();
            ViewData["LoadingPortId"] = new SelectList(ports, "Id", "Name", shipment.LoadingPortId);
            ViewData["DischargePortId"] = new SelectList(ports, "Id", "Name", shipment.DischargePortId);
            return View(shipment);
        }
    }
}