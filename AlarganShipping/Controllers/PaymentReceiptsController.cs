// مسار الملف: Controllers/PaymentReceiptsController.cs
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
    [Authorize]
    public class PaymentReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var receipts = await _context.PaymentReceipts
                .Include(p => p.Customer)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(receipts);
        }

        public IActionResult Create()
        {
            // تعبئة قائمة العملاء
            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentReceipt receipt)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(receipt.ReceiptNumber))
                {
                    receipt.ReceiptNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                if (receipt.PaymentDate == default)
                {
                    receipt.PaymentDate = DateTime.Now;
                }

                _context.Add(receipt);

                // تحديث مديونية العميل
                var customer = await _context.Customers.FindAsync(receipt.CustomerId);
                if (customer != null)
                {
                    customer.TotalPaid += receipt.Amount;
                    customer.TotalBalance -= receipt.Amount;

                    // منع الرصيد من أن يكون بالسالب (إن دفع أكثر يمكن تحويله لرصيد دائن لاحقاً)
                    if (customer.TotalBalance < 0) customer.TotalBalance = 0;

                    _context.Update(customer);
                }

                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name", receipt.CustomerId);
            return View(receipt);
        }
    }
}