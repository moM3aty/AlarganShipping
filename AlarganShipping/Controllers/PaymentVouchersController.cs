using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class PaymentVouchersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentVouchersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة سندات الصرف
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.PaymentVouchers
                .Include(p => p.Customer)
                .OrderByDescending(p => p.VoucherDate)
                .ToListAsync();
            return View(vouchers);
        }

        // شاشة إنشاء سند صرف
        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            return View();
        }

        // حفظ سند الصرف (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentVoucher voucher)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("VoucherNumber"); // تجاوز التحقق
            ModelState.Remove("ReferenceNumber"); // لأنه اختياري

            if (ModelState.IsValid)
            {
                // توليد رقم سند فريد
                voucher.VoucherNumber = "PV-" + DateTime.Now.ToString("yyMMddHHmm");

                // إذا كان الصرف مرتبط بعميل (إرجاع أموال للعميل)
                if (voucher.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(voucher.CustomerId.Value);
                    if (customer != null)
                    {
                        // إرجاع الأموال يقلل من إجمالي ما دفعه العميل لنا
                        customer.TotalPaid -= voucher.Amount;
                        if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                        _context.Update(customer);
                    }
                }

                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // حذف سند صرف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.PaymentVouchers.FindAsync(id);
            if (voucher == null) return Json(new { success = false, message = "السند غير موجود" });

            try
            {
                // إذا كان مرتبط بعميل، نعيد المبلغ لحسابه (لأننا ألغينا الإرجاع)
                if (voucher.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(voucher.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.TotalPaid += voucher.Amount;
                        _context.Update(customer);
                    }
                }

                _context.PaymentVouchers.Remove(voucher);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        // طباعة السند
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();
            var voucher = await _context.PaymentVouchers.Include(v => v.Customer).FirstOrDefaultAsync(m => m.Id == id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }
    }
}