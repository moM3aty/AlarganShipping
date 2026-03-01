using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    // المتحكم المسؤول عن دليل الخدمات والأسعار
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}