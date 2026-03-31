using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class PaymentReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PaymentReceiptsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // عرض قائمة السندات
        public async Task<IActionResult> Index()
        {
            var receipts = await _context.PaymentReceipts
                .Include(p => p.Customer)
                .Include(p => p.Car) // جلب بيانات السيارة المرتبطة
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(receipts);
        }

        // دالة لجلب سيارات العميل فورياً (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCustomerCars(int customerId)
        {
            var cars = await _context.Cars
                .Where(c => c.CustomerId == customerId)
                .Select(c => new {
                    id = c.Id,
                    text = $"{c.Make} {c.Model} ({c.Year}) - VIN: {c.VIN}"
                })
                .ToListAsync();

            return Json(cars);
        }

        // شاشة إضافة سند جديد (مع استقبال invoiceId إن وجد)
        public async Task<IActionResult> Create(int? invoiceId)
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");

            // إذا تم تحويل المستخدم من صفحة تفاصيل الفاتورة، نقوم بتجهيز البيانات المسبقة
            if (invoiceId.HasValue)
            {
                var invoice = await _context.Invoices.Include(i => i.Car).FirstOrDefaultAsync(i => i.Id == invoiceId);
                if (invoice != null)
                {
                    ViewBag.PreSelectedCustomerId = invoice.CustomerId;
                    ViewBag.PreSelectedCarId = invoice.CarId;
                    if (invoice.Car != null)
                    {
                        ViewBag.PreSelectedCarText = $"{invoice.Car.Make} {invoice.Car.Model} ({invoice.Car.Year}) - VIN: {invoice.Car.VIN}";
                    }
                }
            }

            return View();
        }

        // حفظ السند وتحديث حساب العميل (دعم الدفع الجزئي / الأقساط والعملات)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentReceipt receipt, IFormFile? AttachmentFile, string currency = "USD", decimal exchangeRate = 0.386m)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("ReceiptNumber");
            ModelState.Remove("exchangeRate");
            ModelState.Remove("Discount");

            if (ModelState.IsValid)
            {
                // توليد رقم سند فريد
                receipt.ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                // تحويل المبلغ المدخل إلى دولار قبل الحفظ
                if (currency == "OMR" && exchangeRate > 0)
                {
                    decimal originalOmr = receipt.Amount;
                    receipt.Amount = Math.Round(originalOmr / exchangeRate, 2);
                    receipt.Notes += $" \n(ملاحظة مالية: المبلغ المستلم فعلياً {originalOmr} ريال عماني، تم تحويله للدولار بسعر صرف {exchangeRate})";
                }

                // معالجة المرفقات إن وجدت
                if (AttachmentFile != null && AttachmentFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "receipts");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AttachmentFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await AttachmentFile.CopyToAsync(fileStream);
                    }
                    receipt.AttachmentPath = "/uploads/receipts/" + uniqueFileName;
                }

                // تحديث مديونية العميل
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    decimal totalDeduction = receipt.TotalDeducted;

                    customer.TotalPaid += receipt.Amount;
                    customer.TotalBalance -= totalDeduction;

                    if (customer.TotalBalance < 0) customer.TotalBalance = 0;

                    _context.Update(customer);
                }

                _context.Add(receipt);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // شاشة تعديل السند
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var receipt = await _context.PaymentReceipts.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == id);
            if (receipt == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", receipt.CustomerId);

            // جلب سيارات العميل الحالي لملء القائمة المنسدلة في التعديل
            var customerCars = await _context.Cars.Where(c => c.CustomerId == receipt.CustomerId)
                .Select(c => new { Id = c.Id, Text = $"{c.Make} {c.Model} ({c.Year}) - VIN: {c.VIN}" })
                .ToListAsync();
            ViewBag.CustomerCars = new SelectList(customerCars, "Id", "Text", receipt.CarId);

            return View(receipt);
        }

        // حفظ التعديلات وتسوية حسابات العميل مع دعم العملات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentReceipt receipt, IFormFile? AttachmentFile, string currency = "USD", decimal exchangeRate = 0.386m)
        {
            if (id != receipt.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("ReceiptNumber");
            ModelState.Remove("exchangeRate");

            if (ModelState.IsValid)
            {
                try
                {
                    var oldReceipt = await _context.PaymentReceipts.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    if (oldReceipt == null) return Json(new { success = false, errors = new[] { "السند غير موجود." } });

                    receipt.ReceiptNumber = oldReceipt.ReceiptNumber;

                    // معالجة العملة عند التعديل
                    if (currency == "OMR" && exchangeRate > 0)
                    {
                        decimal originalOmr = receipt.Amount;
                        receipt.Amount = Math.Round(originalOmr / exchangeRate, 2);
                        if (receipt.Notes == null || !receipt.Notes.Contains("ريال عماني"))
                            receipt.Notes += $" \n(ملاحظة تعديل: المبلغ المستلم {originalOmr} ريال عماني بسعر صرف {exchangeRate})";
                    }

                    // معالجة الملف الجديد إذا تم رفعه
                    if (AttachmentFile != null && AttachmentFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(oldReceipt.AttachmentPath))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, oldReceipt.AttachmentPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "receipts");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AttachmentFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await AttachmentFile.CopyToAsync(fileStream);
                        }
                        receipt.AttachmentPath = "/uploads/receipts/" + uniqueFileName;
                    }
                    else
                    {
                        receipt.AttachmentPath = oldReceipt.AttachmentPath;
                    }

                    // تسوية حساب العميل
                    if (oldReceipt.CustomerId != receipt.CustomerId)
                    {
                        var oldCustomer = await _context.Customers.FindAsync(oldReceipt.CustomerId);
                        if (oldCustomer != null)
                        {
                            oldCustomer.TotalPaid -= oldReceipt.Amount;
                            oldCustomer.TotalBalance += oldReceipt.TotalDeducted;
                            if (oldCustomer.TotalPaid < 0) oldCustomer.TotalPaid = 0;
                            _context.Update(oldCustomer);
                        }

                        var newCustomer = await _context.Customers.FindAsync(receipt.CustomerId);
                        if (newCustomer != null)
                        {
                            newCustomer.TotalPaid += receipt.Amount;
                            newCustomer.TotalBalance -= receipt.TotalDeducted;
                            if (newCustomer.TotalBalance < 0) newCustomer.TotalBalance = 0;
                            _context.Update(newCustomer);
                        }
                    }
                    else
                    {
                        var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                        if (customer != null)
                        {
                            customer.TotalPaid = customer.TotalPaid - oldReceipt.Amount + receipt.Amount;
                            customer.TotalBalance = customer.TotalBalance + oldReceipt.TotalDeducted - receipt.TotalDeducted;

                            if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                            if (customer.TotalPaid < 0) customer.TotalPaid = 0;

                            _context.Update(customer);
                        }
                    }

                    _context.Update(receipt);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "حدث خطأ: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // شاشة الطباعة الاحترافية للسند
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var receipt = await _context.PaymentReceipts
                .Include(r => r.Customer)
                .Include(r => r.Car) // طباعة الشاصي في السند
                .FirstOrDefaultAsync(m => m.Id == id);

            if (receipt == null) return NotFound();

            return View(receipt);
        }

        // حذف السند
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var receipt = await _context.PaymentReceipts.FindAsync(id);
            if (receipt == null) return Json(new { success = false, message = "السند غير موجود." });

            try
            {
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    customer.TotalPaid -= receipt.Amount;
                    customer.TotalBalance += receipt.TotalDeducted;
                    if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                    _context.Update(customer);
                }

                if (!string.IsNullOrEmpty(receipt.AttachmentPath))
                {
                    var fullPath = Path.Combine(_env.WebRootPath, receipt.AttachmentPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
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