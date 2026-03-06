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

        // ==========================================
        // عرض قائمة الفواتير
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
            return View(invoices);
        }

        // ==========================================
        // شاشة إضافة فاتورة جديدة (GET)
        // ==========================================
        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN");

            // توليد رقم الفاتورة مسبقاً لتجنب خطأ Validation وتسهيل العمل
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-" + DateTime.Now.ToString("yyMM") + "-" + new Random().Next(1000, 9999),
                IssueDate = DateTime.Now
            };

            return View(invoice);
        }

        // ==========================================
        // حفظ الفاتورة الجديدة (POST - AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            // إزالة الحقول المرتبطة لمنع أخطاء التحقق
            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("InvoiceNumber");

            if (ModelState.IsValid)
            {
                // إذا لم يتم تمرير رقم الفاتورة لسبب ما، قم بتوليده
                if (string.IsNullOrEmpty(invoice.InvoiceNumber) || invoice.InvoiceNumber == "INV-AUTO")
                {
                    invoice.InvoiceNumber = "INV-" + DateTime.Now.ToString("yyMM") + "-" + new Random().Next(1000, 9999);
                }

                // حساب الإجمالي: إذا كانت شحن فقط، يتم تجاهل سعر السيارة والمزاد
                decimal carCost = invoice.IsShippingOnly ? 0 : (invoice.CarPrice + invoice.AuctionFees);

                invoice.TotalAmount = carCost +
                                      invoice.InlandTowing +
                                      invoice.SeaFreight +
                                      invoice.CustomsAndTaxes +
                                      invoice.CompanyCommission +
                                      invoice.StorageFees;

                _context.Add(invoice);

                // تحديث مديونية العميل
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance += invoice.TotalAmount;
                    customer.TotalPaid += invoice.AmountPaid; // إضافة ما دفعه مقدماً من الفاتورة
                    _context.Update(customer);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // شاشة تعديل الفاتورة (GET)
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
        // حفظ التعديلات على الفاتورة (POST - AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("InvoiceNumber");

            if (ModelState.IsValid)
            {
                try
                {
                    // جلب الفاتورة القديمة لمعرفة الفروقات المالية
                    var oldInvoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (oldInvoice == null) return Json(new { success = false, errors = new[] { "الفاتورة غير موجودة." } });

                    // الحفاظ على الرقم وتاريخ الإصدار الأصلي
                    invoice.InvoiceNumber = oldInvoice.InvoiceNumber;
                    invoice.IssueDate = oldInvoice.IssueDate;

                    // إعادة حساب الإجمالي
                    decimal carCost = invoice.IsShippingOnly ? 0 : (invoice.CarPrice + invoice.AuctionFees);
                    invoice.TotalAmount = carCost +
                                          invoice.InlandTowing +
                                          invoice.SeaFreight +
                                          invoice.CustomsAndTaxes +
                                          invoice.CompanyCommission +
                                          invoice.StorageFees;

                    // معالجة تغيير العميل أو تغيير المبالغ
                    if (oldInvoice.CustomerId != invoice.CustomerId)
                    {
                        // تم تغيير العميل: إزالة المديونية من القديم وإضافتها للجديد
                        var oldCustomer = await _context.Customers.FindAsync(oldInvoice.CustomerId);
                        if (oldCustomer != null)
                        {
                            oldCustomer.TotalBalance -= oldInvoice.TotalAmount;
                            oldCustomer.TotalPaid -= oldInvoice.AmountPaid;
                            if (oldCustomer.TotalBalance < 0) oldCustomer.TotalBalance = 0;
                            if (oldCustomer.TotalPaid < 0) oldCustomer.TotalPaid = 0;
                            _context.Update(oldCustomer);
                        }

                        var newCustomer = await _context.Customers.FindAsync(invoice.CustomerId);
                        if (newCustomer != null)
                        {
                            newCustomer.TotalBalance += invoice.TotalAmount;
                            newCustomer.TotalPaid += invoice.AmountPaid;
                            _context.Update(newCustomer);
                        }
                    }
                    else
                    {
                        // نفس العميل: إضافة الفارق فقط
                        var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                        if (customer != null)
                        {
                            decimal diffBalance = invoice.TotalAmount - oldInvoice.TotalAmount;
                            decimal diffPaid = invoice.AmountPaid - oldInvoice.AmountPaid;

                            customer.TotalBalance += diffBalance;
                            customer.TotalPaid += diffPaid;

                            if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                            if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                            _context.Update(customer);
                        }
                    }

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
                    return Json(new { success = false, errors = new[] { "حدث خطأ أثناء الحفظ: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // حذف الفاتورة (POST - AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return Json(new { success = false, message = "الفاتورة غير موجودة." });

            try
            {
                // إرجاع مديونية العميل لما كانت عليه قبل الفاتورة
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance -= invoice.TotalAmount;
                    customer.TotalPaid -= invoice.AmountPaid;
                    if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                    if (customer.TotalPaid < 0) customer.TotalPaid = 0;
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

        // ==========================================
        // عرض الفاتورة للطباعة (GET)
        // ==========================================
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