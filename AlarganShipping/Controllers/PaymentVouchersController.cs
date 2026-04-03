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
                .Include(v => v.Car) // جلب بيانات السيارة
                .OrderByDescending(v => v.VoucherDate)
                .ToListAsync();
            return View(vouchers);
        }

        // استقبال carId لكي يتم اختياره تلقائياً إذا تم فتح الصفحة من سيارة محددة
        public IActionResult Create(int? carId)
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");

            // 💡 التعديل هنا: إرسال قائمة السيارات للشاشة باسم CarsList لتجنب تضارب الأسماء
            var carsList = _context.Cars.Select(c => new { Id = c.Id, Text = c.Make + " " + c.Model + " - " + c.VIN }).ToList();
            ViewBag.CarsList = new SelectList(carsList, "Id", "Text", carId);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentVoucher voucher)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Car"); // إزالة الـ Validation المزعج
            ModelState.Remove("VoucherNumber");

            if (ModelState.IsValid)
            {
                voucher.VoucherNumber = "VOU-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                // أتمتة: إذا كان إرجاع أموال أو تعويض، نخصم من إجمالي المدفوع للعميل
                if ((voucher.Category == "إرجاع أموال لعميل" || voucher.Category == "تعويض للعميل عن أضرار") && voucher.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(voucher.CustomerId);
                    if (customer != null)
                    {
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.PaymentVouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", voucher.CustomerId);

            // 💡 التعديل هنا: إرسال قائمة السيارات للشاشة للتعديل
            var carsList = await _context.Cars.Select(c => new { Id = c.Id, Text = c.Make + " " + c.Model + " - " + c.VIN }).ToListAsync();
            ViewBag.CarsList = new SelectList(carsList, "Id", "Text", voucher.CarId);

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentVoucher voucher)
        {
            if (id != voucher.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("VoucherNumber");

            if (ModelState.IsValid)
            {
                try
                {
                    var oldVoucher = await _context.PaymentVouchers.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                    if (oldVoucher == null) return Json(new { success = false, errors = new[] { "السند غير موجود." } });

                    voucher.VoucherNumber = oldVoucher.VoucherNumber;

                    bool isOldRefundOrComp = oldVoucher.Category == "إرجاع أموال لعميل" || oldVoucher.Category == "تعويض للعميل عن أضرار";
                    bool isNewRefundOrComp = voucher.Category == "إرجاع أموال لعميل" || voucher.Category == "تعويض للعميل عن أضرار";

                    if (oldVoucher.CustomerId != voucher.CustomerId || isOldRefundOrComp != isNewRefundOrComp)
                    {
                        if (isOldRefundOrComp && oldVoucher.CustomerId.HasValue)
                        {
                            var oldCustomer = await _context.Customers.FindAsync(oldVoucher.CustomerId);
                            if (oldCustomer != null)
                            {
                                oldCustomer.TotalPaid += oldVoucher.Amount;
                                _context.Update(oldCustomer);
                            }
                        }

                        if (isNewRefundOrComp && voucher.CustomerId.HasValue)
                        {
                            var newCustomer = await _context.Customers.FindAsync(voucher.CustomerId);
                            if (newCustomer != null)
                            {
                                newCustomer.TotalPaid -= voucher.Amount;
                                if (newCustomer.TotalPaid < 0) newCustomer.TotalPaid = 0;
                                _context.Update(newCustomer);
                            }
                        }
                    }
                    else if (isOldRefundOrComp && isNewRefundOrComp && voucher.CustomerId.HasValue)
                    {
                        var customer = await _context.Customers.FindAsync(voucher.CustomerId);
                        if (customer != null)
                        {
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
                .Include(v => v.Car)
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
                if ((voucher.Category == "إرجاع أموال لعميل" || voucher.Category == "تعويض للعميل عن أضرار") && voucher.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(voucher.CustomerId);
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
    }
}