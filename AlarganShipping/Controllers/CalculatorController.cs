using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class CalculatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}