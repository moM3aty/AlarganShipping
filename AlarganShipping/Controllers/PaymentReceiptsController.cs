using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class PaymentReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة السندات
        public async Task<IActionResult> Index()
        {
            var receipts = await _context.PaymentReceipts
                .Include(p => p.Customer)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(receipts);
        }

        // شاشة إضافة سند جديد
        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            return View();
        }

        // حفظ السند وتحديث حساب العميل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentReceipt receipt)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("ReceiptNumber"); // حل مشكلة إجبارية رقم السند
            ModelState.Remove("ReferenceNumber"); // حل مشكلة إجبارية رقم المرجع (رقم الشيك)

            if (ModelState.IsValid)
            {
                // توليد رقم سند فريد دائماً وتجاهل أي قيمة قادمة
                receipt.ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmm");

                // تحديث مديونية العميل بشكل ديناميكي
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    customer.TotalPaid += receipt.Amount;
                    customer.TotalBalance -= receipt.Amount;

                    // منع الديون السالبة
                    if (customer.TotalBalance < 0)
                    {
                        customer.TotalBalance = 0;
                    }
                    _context.Update(customer);
                }

                _context.Add(receipt);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // شاشة التعديل (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var receipt = await _context.PaymentReceipts.FindAsync(id);
            if (receipt == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", receipt.CustomerId);
            return View(receipt);
        }

        // ==========================================
        // حفظ التعديلات (POST) - المنطق المالي الذكي
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentReceipt receipt)
        {
            if (id != receipt.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("ReceiptNumber"); // تجاوز التحقق
            ModelState.Remove("ReferenceNumber"); // تجاوز التحقق

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. جلب السند القديم بدون تتبع لمعرفة التغييرات
                    var oldReceipt = await _context.PaymentReceipts.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    if (oldReceipt == null) return Json(new { success = false, errors = new[] { "السند غير موجود." } });

                    // الحفاظ على رقم السند الأصلي
                    receipt.ReceiptNumber = oldReceipt.ReceiptNumber;

                    // 2. تحديث حسابات العملاء
                    if (oldReceipt.CustomerId != receipt.CustomerId)
                    {
                        // تم تغيير العميل: إرجاع المبلغ للعميل القديم وخصمه من الجديد
                        var oldCustomer = await _context.Customers.FindAsync(oldReceipt.CustomerId);
                        if (oldCustomer != null)
                        {
                            oldCustomer.TotalPaid -= oldReceipt.Amount;
                            oldCustomer.TotalBalance += oldReceipt.Amount;
                            if (oldCustomer.TotalPaid < 0) oldCustomer.TotalPaid = 0;
                            _context.Update(oldCustomer);
                        }

                        var newCustomer = await _context.Customers.FindAsync(receipt.CustomerId);
                        if (newCustomer != null)
                        {
                            newCustomer.TotalPaid += receipt.Amount;
                            newCustomer.TotalBalance -= receipt.Amount;
                            if (newCustomer.TotalBalance < 0) newCustomer.TotalBalance = 0;
                            _context.Update(newCustomer);
                        }
                    }
                    else
                    {
                        // نفس العميل: حساب فارق المبلغ وتحديث الرصيد
                        decimal difference = receipt.Amount - oldReceipt.Amount;
                        var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                        if (customer != null)
                        {
                            customer.TotalPaid += difference;
                            customer.TotalBalance -= difference;
                            if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                            if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                            _context.Update(customer);
                        }
                    }

                    _context.Update(receipt);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PaymentReceipts.Any(e => e.Id == receipt.Id)) return Json(new { success = false, errors = new[] { "السند غير موجود." } });
                    else throw;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "خطأ في حفظ البيانات: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // حذف سند القبض (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var receipt = await _context.PaymentReceipts.FindAsync(id);
            if (receipt == null) return Json(new { success = false, message = "السند غير موجود." });

            try
            {
                // إرجاع مبلغ السند لمديونية العميل (لأننا ألغينا القبض)
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    customer.TotalPaid -= receipt.Amount;
                    customer.TotalBalance += receipt.Amount;

                    if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                    _context.Update(customer);
                }

                _context.PaymentReceipts.Remove(receipt);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }
    }
}