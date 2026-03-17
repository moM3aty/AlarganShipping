using Microsoft.AspNetCore.Mvc;
using AlarganShipping.Models;
using AlarganShipping.Services;
using Microsoft.AspNetCore.Authorization;

namespace AlarganShipping.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ICalculatorService _calculatorService;

        public CalculatorController(ICalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate([FromBody] CalculatorRequest request)
        {
            if (request == null || request.SalePrice < 0) return BadRequest("بيانات غير صالحة");

            var response = _calculatorService.Calculate(request);
            return Json(response);
        }
    }
}