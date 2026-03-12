using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlarganShipping.Controllers
{
    [Authorize] // إضافة حماية للصفحة حتى لا تفتح لغير المسجلين
    public class AccountingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
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
                    if (inv.IssueDate.Month >= 1 && inv.IssueDate.Month <= 12)
                    {
                        monthlySales[inv.IssueDate.Month - 1] += inv.TotalAmount;
                    }
                }

                var carsThisYear = await _context.Cars
                    .Where(c => c.AddDate.HasValue && c.AddDate.Value.Year == currentYear)
                    .ToListAsync();

                foreach (var car in carsThisYear)
                {
                    if (car.AddDate.Value.Month >= 1 && car.AddDate.Value.Month <= 12)
                    {
                        monthlyPurchases[car.AddDate.Value.Month - 1] += car.PurchasePrice;
                    }
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
            catch (Exception ex)
            {
                // في حال حدوث أي خطأ في قاعدة البيانات، نعيد بيانات فارغة لكي لا تنهار الصفحة (Crash)
                ViewBag.ErrorMessage = "حدث خطأ أثناء جلب البيانات المحاسبية: " + ex.Message;

                ViewBag.TotalSales = 0m;
                ViewBag.TotalPaid = 0m;
                ViewBag.PendingCollections = 0m;
                ViewBag.InvoicesCount = 0;
                ViewBag.TotalPurchases = 0m;
                ViewBag.TotalExpenses = 0m;
                ViewBag.PurchasedCarsCount = 0;
                ViewBag.MonthlySales = new decimal[12];
                ViewBag.MonthlyPurchases = new decimal[12];
                ViewBag.CurrentYear = DateTime.Now.Year;

                return View();
            }
        }
    }
}