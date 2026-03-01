using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم الفواتير والماليات الشامل (القسم المالي)
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
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .OrderByDescending(i => i.IssueDate);
            return View(await invoices.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN");
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

                invoice.InvoiceNumber = "INV-" + System.DateTime.Now.ToString("yyyyMMddHHmmss");

                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", invoice.CustomerId);
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", invoice.CarId);
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