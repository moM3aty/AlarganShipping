using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    // المتحكم المسؤول عن حاسبة تكاليف الشحن
    public class CalculatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // في الواقع، يمكن إضافة Action هنا لاستقبال البيانات وحسابها برمجياً
        // لكن التصميم الحالي يعتمد على واجهة تفاعلية.
    }
}