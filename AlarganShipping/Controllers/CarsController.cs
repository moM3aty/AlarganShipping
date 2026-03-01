using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم إدارة السيارات
    [Authorize]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // قائمة جميع السيارات مع ربط اسم العميل والمزاد
        public async Task<IActionResult> Index()
        {
            var cars = _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .Include(c => c.Shipment);
            return View(await cars.ToListAsync());
        }

        public IActionResult Create()
        {
            // تجهيز القوائم المنسدلة لاختيار العميل والمزاد
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            if (ModelState.IsValid)
            {
                car.StatusId = 1; // تعيين الحالة الافتراضية "تم الشراء"
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", car.CustomerId);
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "Id", "Name", car.AuctionId);
            return View(car);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .Include(c => c.Shipment)
                .Include(c => c.TrackingLogs) // إرفاق سجل التتبع
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            return View(car);
        }
    }
}