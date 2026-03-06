using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class TrackingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                ViewBag.Error = "يرجى إدخال رقم الشاصي أو رقم اللوت.";
                return View("Index");
            }

            var cleanVin = vin.Trim();

            // البحث برقم الشاصي، أو الكود الداخلي، أو رقم اللوت (Lot Number)
            // مع جلب كافة البيانات المرتبطة (المزاد، الشحنة، الموانئ، وسجلات التتبع)
            var car = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .Include(c => c.Shipment).ThenInclude(s => s.LoadingPort)
                .Include(c => c.Shipment).ThenInclude(s => s.DischargePort)
                .Include(c => c.TrackingLogs)
                .FirstOrDefaultAsync(c => c.VIN == cleanVin || c.InternalCode == cleanVin || c.LotNumber == cleanVin);

            if (car == null)
            {
                ViewBag.Error = "عذراً، لم يتم العثور على شحنة مطابقة لهذا الرقم.";
                return View("Index");
            }

            return View("Result", car);
        }
    }
}