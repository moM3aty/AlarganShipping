using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.OrderByDescending(c => c.Id).ToListAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            ModelState.Remove("Cars");
            ModelState.Remove("PaymentReceipts");
            ModelState.Remove("Invoices");
            ModelState.Remove("CustomerCode");
            ModelState.Remove("Notifications");

            if (ModelState.IsValid)
            {
                customer.CustomerCode = "CUST-" + new Random().Next(1000, 9999);

                // إذا لم يدخل الموظف يوزر أو باسورد، نقوم بتوليدهم تلقائياً
                if (string.IsNullOrEmpty(customer.PortalUsername))
                    customer.PortalUsername = "user" + new Random().Next(100, 999);
                if (string.IsNullOrEmpty(customer.PortalPassword))
                    customer.PortalPassword = new Random().Next(100000, 999999).ToString();

                customer.TotalBalance = 0;
                customer.TotalPaid = 0;

                try
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "خطأ في قاعدة البيانات." } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Cars");
            ModelState.Remove("PaymentReceipts");
            ModelState.Remove("Invoices");
            ModelState.Remove("Notifications");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer != null)
                    {
                        // تحديث البيانات الأساسية فقط لتجنب تصفير الرصيد
                        existingCustomer.Name = customer.Name;
                        existingCustomer.Phone = customer.Phone;
                        existingCustomer.Email = customer.Email;
                        existingCustomer.CivilId = customer.CivilId;
                        existingCustomer.Address = customer.Address;

                        // تحديث بيانات البوابة
                        existingCustomer.PortalUsername = customer.PortalUsername;
                        if (!string.IsNullOrEmpty(customer.PortalPassword))
                        {
                            existingCustomer.PortalPassword = customer.PortalPassword;
                        }
                        existingCustomer.IsPortalActive = customer.IsPortalActive;

                        _context.Update(existingCustomer);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, errors = new[] { "العميل غير موجود." } });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id)) return Json(new { success = false, errors = new[] { "العميل غير موجود." } });
                    else throw;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "خطأ في قاعدة البيانات." } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }
        public async Task<IActionResult> PrintStatement(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Cars).ThenInclude(car => car.Auction)
                .Include(c => c.Invoices)
                .Include(c => c.PaymentReceipts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }
        // ==========================================
        // إضافة عميل سريع من شاشة السيارات (AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuick(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "الاسم مطلوب" });

            try
            {
                var customer = new Customer
                {
                    Name = name,
                    Phone = "-", // بيانات افتراضية مؤقتة
                    Email = "info@alargan.com", // بريد افتراضي لمنع مشاكل الـ Validation
                    Address = "غير محدد", // <--- حل المشكلة بتمرير قيمة افتراضية للعنوان
                    CivilId = "-", // إضافة قيمة للرقم المدني لتجنب أي أخطاء مشابهة
                    CustomerCode = "CUST-" + new Random().Next(1000, 9999),
                    PortalUsername = "user" + new Random().Next(100, 999),
                    PortalPassword = new Random().Next(100000, 999999).ToString(),
                    IsPortalActive = true
                };

                _context.Add(customer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = customer.Id, name = customer.Name });
            }
            catch (Exception ex)
            {
                // إرجاع رسالة الخطأ الدقيقة لتظهر في السكريبت
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return Json(new { success = false, message = "العميل غير موجود." });

            bool hasCars = await _context.Cars.AnyAsync(c => c.CustomerId == id);
            if (hasCars) return Json(new { success = false, message = "لا يمكن حذف العميل لارتباطه بسيارات." });

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "لا يمكن الحذف لوجود ارتباطات مالية." });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers
                .Include(c => c.Cars)
                .Include(c => c.Invoices)
                .Include(c => c.PaymentReceipts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();
            return View(customer);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}