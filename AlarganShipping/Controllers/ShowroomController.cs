using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            // 1. جلب ايدي عميل "معرض الشركة"
            int inventoryCustomerId = await GetOrCreateCompanyInventoryCustomerId();

            // 2. جلب سيارات الشركة المعروضة للبيع
            var cars = await _context.Cars
                .Where(c => c.CustomerId == inventoryCustomerId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            // 3. جلب قائمة العملاء الحقيقيين (لاستخدامهم في قائمة اختيار العميل عند البيع)
            var realCustomers = await _context.Customers
                .Where(c => c.Id != inventoryCustomerId)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();

            ViewBag.RealCustomers = realCustomers;

            return View(cars);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SellCarToCustomer(int carId, int newCustomerId, decimal sellingPrice)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                if (car == null) return Json(new { success = false, message = "السيارة غير موجودة." });

                var customer = await _context.Customers.FindAsync(newCustomerId);
                if (customer == null) return Json(new { success = false, message = "العميل غير موجود." });

                // تحديث بيانات السيارة لتعود ملكيتها للعميل الجديد وتسجيل سعر البيع
                car.CustomerId = newCustomerId;
                car.SellingPrice = sellingPrice;

                // تسجيل حركة التتبع للمشتري الجديد
                _context.TrackingLogs.Add(new TrackingLog
                {
                    CarId = car.Id,
                    Title = "تم الشراء من معرض الشركة",
                    Description = $"تم انتقال ملكية السيارة للعميل: {customer.Name}",
                    Location = "المكتب الرئيسي",
                    UpdateDate = DateTime.Now,
                    ProgressPercentage = 10
                });

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء نقل الملكية: " + ex.Message });
            }
        }

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
                    PortalUsername = "inventory_system_" + Guid.NewGuid().ToString().Substring(0, 5),
                    PortalPassword = Guid.NewGuid().ToString().Substring(0, 8),
                    IsPortalActive = false,
                    TotalBalance = 0,
                    TotalPaid = 0
                };
                _context.Customers.Add(companyCustomer);
                await _context.SaveChangesAsync();
            }

            return companyCustomer.Id;
        }
    }
}