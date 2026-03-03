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

            if (ModelState.IsValid)
            {
                // توليد رقم سند فريد
                receipt.ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmm");

                // تحديث مديونية العميل بشكل ديناميكي
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    customer.TotalPaid += receipt.Amount;
                    customer.TotalBalance -= receipt.Amount;

                    // منع الديون السالبة (في حال دفع مبلغ أكبر من دينه)
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
    }
}