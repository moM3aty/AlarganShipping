using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class PaymentVouchersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentVouchersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.PaymentVouchers
                .Include(v => v.Customer)
                .OrderByDescending(v => v.VoucherDate)
                .ToListAsync();
            return View(vouchers);
        }

        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentVoucher voucher)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("VoucherNumber");

            if (ModelState.IsValid)
            {
                voucher.VoucherNumber = "VOU-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                // 💡 أتمتة: إذا كان إرجاع أموال، نخصم من إجمالي المدفوع للعميل
                if (voucher.Category == "إرجاع أموال لعميل" && voucher.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(voucher.CustomerId);
                    if (customer != null)
                    {
                        customer.TotalPaid -= voucher.Amount; // تقليل المدفوع (التأثير المالي)
                        if (customer.TotalPaid < 0) customer.TotalPaid = 0; // حماية من القيم السالبة
                        _context.Update(customer);
                    }
                }

                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.PaymentVouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", voucher.CustomerId);
            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentVoucher voucher)
        {
            if (id != voucher.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("VoucherNumber");

            if (ModelState.IsValid)
            {
                try
                {
                    var oldVoucher = await _context.PaymentVouchers.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                    if (oldVoucher == null) return Json(new { success = false, errors = new[] { "السند غير موجود." } });

                    voucher.VoucherNumber = oldVoucher.VoucherNumber;

                    // 💡 تسوية الحسابات باحترافية (Edit Action Logic)
                    if (oldVoucher.CustomerId != voucher.CustomerId || oldVoucher.Category != voucher.Category)
                    {
                        // 1. التراجع عن تأثير السند القديم للعميل القديم
                        if (oldVoucher.Category == "إرجاع أموال لعميل" && oldVoucher.CustomerId.HasValue)
                        {
                            var oldCustomer = await _context.Customers.FindAsync(oldVoucher.CustomerId);
                            if (oldCustomer != null)
                            {
                                oldCustomer.TotalPaid += oldVoucher.Amount; // التراجع عن الخصم
                                _context.Update(oldCustomer);
                            }
                        }

                        // 2. تطبيق تأثير السند الجديد للعميل الجديد
                        if (voucher.Category == "إرجاع أموال لعميل" && voucher.CustomerId.HasValue)
                        {
                            var newCustomer = await _context.Customers.FindAsync(voucher.CustomerId);
                            if (newCustomer != null)
                            {
                                newCustomer.TotalPaid -= voucher.Amount; // تطبيق التأثير
                                if (newCustomer.TotalPaid < 0) newCustomer.TotalPaid = 0;
                                _context.Update(newCustomer);
                            }
                        }
                    }
                    else if (oldVoucher.Category == "إرجاع أموال لعميل" && voucher.Category == "إرجاع أموال لعميل" && voucher.CustomerId.HasValue)
                    {
                        // 3. نفس العميل ونفس التصنيف، نعدل الفارق فقط لضمان سلامة العمليات
                        var customer = await _context.Customers.FindAsync(voucher.CustomerId);
                        if (customer != null)
                        {
                            // نرجع المبلغ القديم، ونخصم المبلغ الجديد
                            customer.TotalPaid = customer.TotalPaid + oldVoucher.Amount - voucher.Amount;
                            if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                            _context.Update(customer);
                        }
                    }

                    _context.Update(voucher);
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

        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.PaymentVouchers
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (voucher == null) return NotFound();

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.PaymentVouchers.FindAsync(id);
            if (voucher == null) return Json(new { success = false, message = "السند غير موجود." });

            try
            {
                // 💡 التراجع المالي عند الحذف
                if (voucher.Category == "إرجاع أموال لعميل" && voucher.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(voucher.CustomerId);
                    if (customer != null)
                    {
                        customer.TotalPaid += voucher.Amount; // التراجع عن الخصم ورد المبالغ المسترجعة للرصيد
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
    }
}