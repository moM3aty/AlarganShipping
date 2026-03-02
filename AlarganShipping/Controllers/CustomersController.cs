// مسار الملف: Controllers/CustomersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AlarganShipping.Models;
using System.Linq;
using System;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة العملاء
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                                          .OrderByDescending(c => c.Id)
                                          .ToListAsync();
            return View(customers);
        }

        // شاشة إضافة عميل جديد
        public IActionResult Create()
        {
            return View();
        }

        // حفظ بيانات العميل الجديد (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            // 💡 الحل الجذري والنهائي: مسح كافة أخطاء الـ Validation لأي حقل غير موجود في الفورم
            var keys = ModelState.Keys.ToList();
            foreach (var key in keys)
            {
                if (key != "Name" && key != "Phone" && key != "Email" && key != "CivilId" && key != "Address")
                {
                    ModelState.Remove(key);
                }
            }

            if (ModelState.IsValid)
            {
                // توليد كود العميل تلقائياً إذا لم يتم إدخاله
                if (string.IsNullOrEmpty(customer.CustomerCode))
                {
                    customer.CustomerCode = "CUST-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                // تعيين قيم افتراضية للبوابة
                if (string.IsNullOrEmpty(customer.PortalUsername))
                {
                    customer.PortalUsername = customer.Phone?.Replace("+", "").Replace(" ", "") ?? "user" + DateTime.Now.ToString("HHmmss");
                }
                if (string.IsNullOrEmpty(customer.PortalPassword))
                {
                    customer.PortalPassword = "password123";
                }

                try
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync(); // الحفظ الفعلي

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // إذا حدث خطأ في قاعدة البيانات نفسها
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, errors = new[] { ex.InnerException?.Message ?? ex.Message } });
                }
            }

            // إرجاع الأخطاء الحقيقية للواجهة
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }

            return View(customer);
        }

        // عرض تفاصيل العميل
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Cars) // ربط السيارات
                .Include(c => c.PaymentReceipts) // ربط المدفوعات
                .Include(c => c.Invoices) // ربط الفواتير
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }
    }
}