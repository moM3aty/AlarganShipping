using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

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

        // حفظ السند وتحديث حساب العميل (دعم الدفع الجزئي / الأقساط)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentReceipt receipt, IFormFile? AttachmentFile)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("ReceiptNumber");

            if (ModelState.IsValid)
            {
                // توليد رقم سند فريد
                receipt.ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss");

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

                // تحديث مديونية العميل (نظام الأقساط والدفع الجزئي)
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    // العميل يستفيد من المبلغ المدفوع + الخصم المسموح به لتقليل دينه
                    decimal totalDeduction = receipt.Amount + receipt.Discount;

                    customer.TotalPaid += receipt.Amount;
                    customer.TotalBalance -= totalDeduction;

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

        // شاشة الطباعة الاحترافية للسند
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var receipt = await _context.PaymentReceipts
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (receipt == null) return NotFound();

            return View(receipt);
        }

        // (يجب أن تظل دوال Edit و Delete موجودة كما كانت في الكود السابق لديك لحذف أو تعديل السندات)
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
                    decimal totalDeduction = receipt.Amount + receipt.Discount;
                    customer.TotalPaid -= receipt.Amount;
                    customer.TotalBalance += totalDeduction;
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