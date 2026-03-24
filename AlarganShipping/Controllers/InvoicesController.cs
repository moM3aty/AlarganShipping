using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

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

        public IActionResult Create(int? carId, int? customerId)
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", customerId);
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN", carId);

            var invoice = new Invoice
            {
                InvoiceNumber = "INV-" + DateTime.Now.ToString("yyMM") + "-" + new Random().Next(1000, 9999),
                IssueDate = DateTime.Now
            };

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("InvoiceNumber");
            ModelState.Remove("TotalAmount");

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(invoice.InvoiceNumber) || invoice.InvoiceNumber == "INV-AUTO")
                {
                    invoice.InvoiceNumber = "INV-" + DateTime.Now.ToString("yyMM") + "-" + new Random().Next(1000, 9999);
                }

                decimal carCost = invoice.IsShippingOnly ? 0 : (invoice.CarPrice + invoice.AuctionFees);

                // إضافة البنود الثلاثة الجديدة للعملية الحسابية
                invoice.TotalAmount = carCost + invoice.InlandTowing + invoice.SeaFreight +
                                      invoice.CustomsAndTaxes + invoice.CompanyCommission + invoice.StorageFees +
                                      invoice.TransferFees + invoice.OmanTowingFees + invoice.CarSizeFees;

                _context.Add(invoice);

                // 💡 أتمتة: تحديث حساب العميل وإرسال إشعار فاتورة
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance += invoice.TotalAmount;
                    customer.TotalPaid += invoice.AmountPaid; // في حالة دفع مقدم
                    _context.Update(customer);

                    _context.Notifications.Add(new Notification
                    {
                        CustomerId = customer.Id,
                        Title = "فاتورة جديدة",
                        Message = $"تم إصدار فاتورة جديدة برقم {invoice.InvoiceNumber} بقيمة {invoice.TotalAmount.ToString("N2")}$. يرجى المراجعة والسداد.",
                        Type = "InvoiceAlert"
                    });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", invoice.CustomerId);
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN", invoice.CarId);

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Customer");
            ModelState.Remove("Car");
            ModelState.Remove("InvoiceNumber");
            ModelState.Remove("TotalAmount");

            if (ModelState.IsValid)
            {
                try
                {
                    var oldInvoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (oldInvoice == null) return Json(new { success = false, errors = new[] { "الفاتورة غير موجودة." } });

                    invoice.InvoiceNumber = oldInvoice.InvoiceNumber;
                    invoice.IssueDate = oldInvoice.IssueDate;

                    decimal carCost = invoice.IsShippingOnly ? 0 : (invoice.CarPrice + invoice.AuctionFees);

                    // إضافة البنود الثلاثة الجديدة في التعديل
                    invoice.TotalAmount = carCost + invoice.InlandTowing + invoice.SeaFreight +
                                          invoice.CustomsAndTaxes + invoice.CompanyCommission + invoice.StorageFees +
                                          invoice.TransferFees + invoice.OmanTowingFees + invoice.CarSizeFees;

                    // 💡 أتمتة: تعديل الفروقات المحاسبية بذكاء
                    if (oldInvoice.CustomerId != invoice.CustomerId)
                    {
                        var oldCustomer = await _context.Customers.FindAsync(oldInvoice.CustomerId);
                        if (oldCustomer != null)
                        {
                            oldCustomer.TotalBalance -= oldInvoice.TotalAmount;
                            oldCustomer.TotalPaid -= oldInvoice.AmountPaid;
                            if (oldCustomer.TotalBalance < 0) oldCustomer.TotalBalance = 0;
                            _context.Update(oldCustomer);
                        }

                        var newCustomer = await _context.Customers.FindAsync(invoice.CustomerId);
                        if (newCustomer != null)
                        {
                            newCustomer.TotalBalance += invoice.TotalAmount;
                            newCustomer.TotalPaid += invoice.AmountPaid;
                            _context.Update(newCustomer);
                        }
                    }
                    else
                    {
                        var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                        if (customer != null)
                        {
                            decimal diffBalance = invoice.TotalAmount - oldInvoice.TotalAmount;
                            decimal diffPaid = invoice.AmountPaid - oldInvoice.AmountPaid;

                            customer.TotalBalance += diffBalance;
                            customer.TotalPaid += diffPaid;
                            if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                            _context.Update(customer);
                        }
                    }

                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "حدث خطأ أثناء الحفظ: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return Json(new { success = false, message = "الفاتورة غير موجودة." });

            try
            {
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance -= invoice.TotalAmount;
                    customer.TotalPaid -= invoice.AmountPaid;
                    if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                    if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                    _context.Update(customer);
                }

                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var invoice = await _context.Invoices.Include(i => i.Customer).Include(i => i.Car).FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        public async Task<IActionResult> DetailsByCar(int carId)
        {
            var invoice = await _context.Invoices.Include(i => i.Customer).Include(i => i.Car).FirstOrDefaultAsync(i => i.CarId == carId);
            if (invoice == null)
            {
                TempData["Message"] = "لم يتم إصدار فاتورة مبيعات لهذه السيارة بعد. يرجى إصدارها أولاً.";
                return RedirectToAction("Create", new { carId = carId });
            }
            return View("DetailsByCar", invoice);
        }

        public async Task<IActionResult> EditByCar(int carId)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.CarId == carId);
            if (invoice == null)
            {
                TempData["Message"] = "لم يتم إصدار فاتورة مبيعات لهذه السيارة لتعديلها.";
                return RedirectToAction("Index", "Cars");
            }
            return RedirectToAction("Edit", new { id = invoice.Id });
        }
    }
}