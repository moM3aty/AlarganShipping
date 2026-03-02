// مسار الملف: Controllers/InvoicesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;
using System;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
            return View(invoices);
        }

        public IActionResult Create()
        {
            // تعبئة القوائم المنسدلة للعملاء والسيارات
            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name");
            // عرض رقم الشاصي مع موديل السيارة لسهولة البحث
            var carsList = _context.Cars.Select(c => new { Id = c.Id, DisplayName = c.Make + " " + c.Model + " (" + c.VIN + ")" }).ToList();
            ViewData["CarId"] = new SelectList(carsList, "Id", "DisplayName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                // الحساب التلقائي للإجمالي
                invoice.TotalAmount = invoice.CarPrice + invoice.AuctionFees +
                                      invoice.LandFreight + invoice.SeaFreight +
                                      invoice.CustomsFees + invoice.AdminFees;

                // تحديث مديونية العميل
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance += invoice.TotalAmount;
                    _context.Update(customer);
                }

                if (string.IsNullOrEmpty(invoice.InvoiceNumber))
                {
                    invoice.InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                if (invoice.IssueDate == default)
                {
                    invoice.IssueDate = DateTime.Now;
                }

                _context.Add(invoice);
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

            ViewData["CustomerId"] = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name", invoice.CustomerId);
            var carsList = _context.Cars.Select(c => new { Id = c.Id, DisplayName = c.Make + " " + c.Model + " (" + c.VIN + ")" }).ToList();
            ViewData["CarId"] = new SelectList(carsList, "Id", "DisplayName", invoice.CarId);
            return View(invoice);
        }

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