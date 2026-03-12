using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class ShowroomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowroomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. دالة مساعدة للحصول على (أو إنشاء) حساب المخزون الخاص بالشركة
        private async Task<int> GetOrCreateCompanyInventoryCustomerId()
        {
            string inventoryName = "سيارات الشركة (المخزون)";
            var companyCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == inventoryName);

            if (companyCustomer == null)
            {
                companyCustomer = new Customer
                {
                    Name = inventoryName,
                    Phone = "-",
                    Email = "inventory@alargan.com",
                    CivilId = "SYSTEM",
                    Address = "مخزون الشركة الداخلي",
                    CustomerCode = "SYS-INV-001",
                    PortalUsername = "inventory_system",
                    PortalPassword = Guid.NewGuid().ToString().Substring(0, 8), // كلمة سر عشوائية للنظام
                    IsPortalActive = false,
                    TotalBalance = 0,
                    TotalPaid = 0
                };
                _context.Customers.Add(companyCustomer);
                await _context.SaveChangesAsync();
            }

            return companyCustomer.Id;
        }

        // 2. عرض سيارات المعرض
        public async Task<IActionResult> Index()
        {
            int inventoryId = await GetOrCreateCompanyInventoryCustomerId();

            // جلب السيارات التي يملكها حساب "المخزون"
            var showroomCars = await _context.Cars
                .Include(c => c.Auction)
                .Include(c => c.Shipment)
                .Where(c => c.CustomerId == inventoryId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            // جلب قائمة العملاء الحقيقيين لاستخدامها في نافذة "البيع"
            ViewBag.RealCustomers = await _context.Customers
                .Where(c => c.Id != inventoryId)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(showroomCars);
        }

        // 3. دالة نقل ملكية السيارة لعميل جديد (عملية البيع)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SellCarToCustomer(int carId, int newCustomerId, decimal sellingPrice)
        {
            int inventoryId = await GetOrCreateCompanyInventoryCustomerId();
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId && c.CustomerId == inventoryId);

            if (car == null)
                return Json(new { success = false, message = "السيارة غير موجودة في المخزون." });

            var newCustomer = await _context.Customers.FindAsync(newCustomerId);
            if (newCustomer == null)
                return Json(new { success = false, message = "العميل المختار غير موجود." });

            try
            {
                // نقل الملكية
                car.CustomerId = newCustomerId;
                car.SellingPrice = sellingPrice;

                // حساب الربح المتوقع
                car.EstimatedProfit = sellingPrice - car.PurchasePrice;

                // تسجيل حركة تتبع
                _context.TrackingLogs.Add(new TrackingLog
                {
                    CarId = car.Id,
                    Title = "تم بيع السيارة",
                    Description = $"تم بيع السيارة من معرض الشركة إلى العميل: {newCustomer.Name}",
                    Location = "معرض الأرجان",
                    ProgressPercentage = car.StatusId >= 4 ? 100 : 50, // يعتمد على مكانها الحالي
                    UpdateDate = DateTime.Now
                });

                await _context.SaveChangesAsync();

                return Json(new { success = true, carId = car.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء إتمام عملية البيع." });
            }
        }
    }
}