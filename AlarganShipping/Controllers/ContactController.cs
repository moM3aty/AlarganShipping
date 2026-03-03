using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}