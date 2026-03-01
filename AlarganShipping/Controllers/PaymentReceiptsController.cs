using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم سندات القبض والمدفوعات
    [Authorize]
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
            var receipts = _context.PaymentReceipts
                .Include(p => p.Customer)
                .OrderByDescending(p => p.PaymentDate);
            return View(await receipts.ToListAsync());
        }

        // شاشة إصدار سند قبض جديد
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            return View();
        }

        // حفظ السند وتحديث رصيد العميل تلقائياً
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentReceipt receipt)
        {
            if (ModelState.IsValid)
            {
                // توليد رقم سند فريد
                receipt.ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                _context.Add(receipt);

                // تحديث مديونية العميل
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    customer.TotalPaid += receipt.Amount;
                    customer.TotalBalance -= receipt.Amount;

                    // منع الرصيد من أن يكون بالسالب إذا دفع أكثر من المطلوب (يمكن تحويله لرصيد دائن مستقبلاً)
                    if (customer.TotalBalance < 0) customer.TotalBalance = 0;

                    _context.Update(customer);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", receipt.CustomerId);
            return View(receipt);
        }
    }
}