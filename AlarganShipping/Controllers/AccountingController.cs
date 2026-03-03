// مسار الملف: Controllers/AccountingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class AccountingController : Controller
    {
        public IActionResult Index()
        {
            // تمرير إحصائيات وهمية مبدئياً لتطابق الصورة
            ViewBag.TotalSales = 41130;
            ViewBag.TotalPurchases = 32750;
            ViewBag.PendingCollections = 5050;
            ViewBag.TotalExpenses = 1250;
            return View();
        }
    }
}