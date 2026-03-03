using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض سجل الفواتير
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
            return View(invoices);
        }

        // شاشة إصدار فاتورة
        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN");
            return View();
        }

        // حفظ الفاتورة وتحديث المديونية (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Car");

            if (ModelState.IsValid)
            {
                invoice.IssueDate = DateTime.Now;
                invoice.InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmm");

                // إضافة إجمالي الفاتورة إلى ديون العميل (TotalBalance)
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance += invoice.TotalAmount;
                    _context.Update(customer);
                }

                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, errors = errors });
        }

        // ==========================================
        // شاشة تعديل فاتورة (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", invoice.CustomerId);
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN", invoice.CarId);
            return View(invoice);
        }

        // ==========================================
        // حفظ التعديلات وتحديث المديونية (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("Car");

            if (ModelState.IsValid)
            {
                try
                {
                    // جلب الفاتورة القديمة من الداتابيز لمعرفة الفرق في المبلغ ولمعرفة إذا تم تغيير العميل
                    var existingInvoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (existingInvoice == null) return Json(new { success = false, errors = new[] { "الفاتورة غير موجودة." } });

                    // إذا تم تغيير العميل المرتبط بالفاتورة
                    if (existingInvoice.CustomerId != invoice.CustomerId)
                    {
                        // 1. خصم المبلغ من العميل القديم
                        var oldCustomer = await _context.Customers.FindAsync(existingInvoice.CustomerId);
                        if (oldCustomer != null)
                        {
                            oldCustomer.TotalBalance -= existingInvoice.TotalAmount;
                            if (oldCustomer.TotalBalance < 0) oldCustomer.TotalBalance = 0;
                            _context.Update(oldCustomer);
                        }

                        // 2. إضافة المبلغ الجديد للعميل الجديد
                        var newCustomer = await _context.Customers.FindAsync(invoice.CustomerId);
                        if (newCustomer != null)
                        {
                            newCustomer.TotalBalance += invoice.TotalAmount;
                            _context.Update(newCustomer);
                        }
                    }
                    else
                    {
                        // إذا كان نفس العميل، نحسب فرق المبلغ (زيادة أو نقصان)
                        decimal difference = invoice.TotalAmount - existingInvoice.TotalAmount;
                        var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                        if (customer != null)
                        {
                            customer.TotalBalance += difference;
                            if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                            _context.Update(customer);
                        }
                    }

                    // الاحتفاظ برقم الفاتورة وتاريخها الأصلي حتى لا يضيع
                    invoice.InvoiceNumber = existingInvoice.InvoiceNumber;
                    invoice.IssueDate = existingInvoice.IssueDate;

                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Invoices.Any(e => e.Id == invoice.Id)) return Json(new { success = false, errors = new[] { "الفاتورة غير موجودة." } });
                    else throw;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "حدث خطأ أثناء حفظ الفاتورة." } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // حذف الفاتورة (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return Json(new { success = false, message = "الفاتورة غير موجودة." });

            try
            {
                // خصم مبلغ الفاتورة من مديونية العميل عند حذفها
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance -= invoice.TotalAmount;
                    if (customer.TotalBalance < 0) customer.TotalBalance = 0; // حماية
                    _context.Update(customer);
                }

                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        // عرض تفاصيل الفاتورة للطباعة
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }
    }
}