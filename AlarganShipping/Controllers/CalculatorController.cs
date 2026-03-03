using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalculatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. جلب المزادات لاستخدام رسومها الافتراضية في الحاسبة
            ViewBag.Auctions = await _context.Auctions.ToListAsync();

            // 2. جلب موانئ الوصول (خارج أمريكا) ليختار منها العميل
            ViewBag.DestinationPorts = await _context.Locations
                .Where(l => l.LocationType == "Port" && l.Country != "USA")
                .ToListAsync();

            return View();
        }
    }
}