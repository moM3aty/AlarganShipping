using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    [Authorize(Roles = "Customer")] // هذه الصفحة للعملاء فقط
    public class CustomerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // جلب رقم العميل المسجل حالياً من الجلسة (Claims)
        private int GetCurrentCustomerId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(idClaim, out int customerId);
            return customerId;
        }

        public async Task<IActionResult> Index()
        {
            int customerId = GetCurrentCustomerId();

            var customer = await _context.Customers
                .Include(c => c.Cars).ThenInclude(car => car.Shipment)
                .Include(c => c.Cars).ThenInclude(car => car.Auction)
                .Include(c => c.Invoices)
                .Include(c => c.PaymentReceipts)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // عرض تفاصيل التتبع للعميل لسيارة محددة
        public async Task<IActionResult> TrackCar(int id)
        {
            int customerId = GetCurrentCustomerId();

            var car = await _context.Cars
                .Include(c => c.Auction)
                .Include(c => c.Shipment).ThenInclude(s => s.DischargePort)
                .Include(c => c.Shipment).ThenInclude(s => s.LoadingPort)
                .Include(c => c.TrackingLogs)
                .Include(c => c.DocumentAttachments) // للسماح للعميل برؤية صور سيارته
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == customerId); // الحماية: يجب أن تكون سيارته

            if (car == null) return Unauthorized();

            return View(car);
        }
    }
}