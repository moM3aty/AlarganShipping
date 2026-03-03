using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    [Authorize(Roles = "Admin")] // الإعدادات للمدير فقط
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}