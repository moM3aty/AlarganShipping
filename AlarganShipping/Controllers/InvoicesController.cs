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

        // عرض سجل الفواتير
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
            return View(invoices);
        }

        // شاشة إصدار فاتورة
        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN");
            return View();
        }

        // حفظ الفاتورة وتحديث المديونية (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("InvoiceNumber");

            if (ModelState.IsValid)
            {
                invoice.IssueDate = DateTime.Now;
                invoice.InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmm");

                // إضافة إجمالي الفاتورة إلى ديون العميل (TotalBalance)
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance += invoice.TotalAmount;
                    _context.Update(customer);
                }

                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, errors = errors });
        }

        // عرض تفاصيل الفاتورة للطباعة
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