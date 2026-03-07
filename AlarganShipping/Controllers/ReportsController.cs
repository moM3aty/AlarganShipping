using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarganShipping.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Customers = new SelectList(await _context.Customers.ToListAsync(), "Id", "Name");
            ViewBag.CarShapes = new SelectList(await _context.Cars.Select(c => c.Make).Distinct().ToListAsync()); // نعتمد على الماركة في التصفية
            return View();
        }

        // 1. تقرير المبيعات (الفواتير)
        [HttpPost]
        public async Task<IActionResult> GetSalesReport(DateTime startDate, DateTime endDate)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Car)
                .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate)
                .OrderBy(i => i.IssueDate)
                .Select(i => new {
                    date = i.IssueDate.ToString("yyyy-MM-dd"),
                    invoiceNo = i.InvoiceNumber,
                    customer = i.Customer.Name,
                    car = i.Car.Make + " " + i.Car.Model,
                    total = i.TotalAmount,
                    paid = i.AmountPaid,
                    remain = i.TotalAmount - i.AmountPaid
                })
                .ToListAsync();

            return Json(invoices);
        }

        // 2. كشف حساب العميل (سندات وفواتير)
        [HttpPost]
        public async Task<IActionResult> GetCustomerStatement(int customerId, DateTime startDate, DateTime endDate)
        {
            // جلب الفواتير (مديونية)
            var invoices = await _context.Invoices
                .Where(i => i.CustomerId == customerId && i.IssueDate >= startDate && i.IssueDate <= endDate)
                .Select(i => new {
                    date = i.IssueDate,
                    type = "فاتورة",
                    desc = "مبيعات فاتورة رقم " + i.InvoiceNumber,
                    debit = i.TotalAmount, // عليه
                    credit = i.AmountPaid  // له (الدفعات المقدمة داخل الفاتورة)
                }).ToListAsync();

            // جلب سندات القبض (دفعات)
            var receipts = await _context.PaymentReceipts
                .Where(r => r.CustomerId == customerId && r.PaymentDate >= startDate && r.PaymentDate <= endDate)
                .Select(r => new {
                    date = r.PaymentDate,
                    type = "سند قبض",
                    desc = "دفعة " + r.PaymentMethod + " " + r.ReferenceNumber,
                    debit = 0m,
                    credit = r.Amount // له
                }).ToListAsync();

            // دمج القائمتين وترتيبهما بالتاريخ
            var statement = invoices.Concat(receipts).OrderBy(x => x.date).Select(x => new {
                date = x.date.ToString("yyyy-MM-dd"),
                type = x.type,
                desc = x.desc,
                debit = x.debit,
                credit = x.credit
            }).ToList();

            // جلب الرصيد الحالي للعميل
            var customer = await _context.Customers.FindAsync(customerId);
            decimal currentBalance = customer?.TotalBalance ?? 0;

            return Json(new { statement = statement, currentBalance = currentBalance });
        }

        // 3. تقرير المشتريات (السيارات)
        [HttpPost]
        public async Task<IActionResult> GetPurchasesReport(string make, DateTime startDate, DateTime endDate)
        {
            var query = _context.Cars.Include(c => c.Auction).AsQueryable();

            query = query.Where(c => c.AddDate >= startDate && c.AddDate <= endDate);

            if (!string.IsNullOrEmpty(make))
            {
                query = query.Where(c => c.Make == make);
            }

            var purchases = await query
                .OrderBy(c => c.AddDate)
                .Select(c => new {
                    date = c.AddDate.HasValue ? c.AddDate.Value.ToString("yyyy-MM-dd") : "-",
                    vin = c.VIN,
                    carInfo = c.Make + " " + c.Model + " " + c.Year,
                    auction = c.Auction.Name,
                    price = c.PurchasePrice
                })
                .ToListAsync();

            return Json(purchases);
        }
    }
}