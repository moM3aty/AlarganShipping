using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class AccountingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentYear = DateTime.Now.Year;

            // 1. حسابات المبيعات والديون (من الفواتير وسندات القبض)
            decimal totalSales = await _context.Invoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
            decimal totalPaid = await _context.PaymentReceipts.SumAsync(p => (decimal?)p.Amount) ?? 0;
            int invoicesCount = await _context.Invoices.CountAsync();
            decimal pendingCollections = totalSales - totalPaid;
            if (pendingCollections < 0) pendingCollections = 0;

            // 2. حسابات المشتريات والمصاريف (من السيارات وأوامر النقل)
            decimal totalPurchases = await _context.Cars.SumAsync(c => (decimal?)c.PurchasePrice) ?? 0;
            int purchasedCarsCount = await _context.Cars.CountAsync();
            decimal totalExpenses = await _context.DispatchOrders.SumAsync(d => (decimal?)d.TowingFee) ?? 0;

            // 3. تجهيز بيانات الرسم البياني (شهرياً للسنة الحالية)
            var monthlySales = new decimal[12];
            var monthlyPurchases = new decimal[12];

            var invoicesThisYear = await _context.Invoices
                .Where(i => i.IssueDate.Year == currentYear)
                .ToListAsync();

            foreach (var inv in invoicesThisYear)
            {
                monthlySales[inv.IssueDate.Month - 1] += inv.TotalAmount;
            }

            var carsThisYear = await _context.Cars
                .Where(c => c.AddDate.HasValue && c.AddDate.Value.Year == currentYear)
                .ToListAsync();

            foreach (var car in carsThisYear)
            {
                monthlyPurchases[car.AddDate.Value.Month - 1] += car.PurchasePrice;
            }

            // تمرير البيانات للواجهة
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.PendingCollections = pendingCollections;
            ViewBag.InvoicesCount = invoicesCount;

            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.PurchasedCarsCount = purchasedCarsCount;

            ViewBag.MonthlySales = monthlySales;
            ViewBag.MonthlyPurchases = monthlyPurchases;
            ViewBag.CurrentYear = currentYear;

            return View();
        }
    }
}