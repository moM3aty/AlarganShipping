using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    // المتحكم المسؤول عن صفحة اتصل بنا
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}