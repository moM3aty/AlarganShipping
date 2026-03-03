using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class TransportersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض القائمة (GET)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Transporters.ToListAsync());
        }

        // شاشة الإضافة (GET)
        public IActionResult Create()
        {
            return View();
        }

        // حفظ ناقل جديد (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transporter transporter)
        {
            ModelState.Remove("DispatchOrders");

            if (ModelState.IsValid)
            {
                _context.Add(transporter);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // شاشة التعديل (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transporter = await _context.Transporters.FindAsync(id);
            if (transporter == null) return NotFound();

            return View(transporter);
        }

        // حفظ التعديلات (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transporter transporter)
        {
            if (id != transporter.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("DispatchOrders");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transporter);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransporterExists(transporter.Id)) return Json(new { success = false, errors = new[] { "الناقل غير موجود." } });
                    else throw;
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // حذف ناقل (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var transporter = await _context.Transporters.FindAsync(id);
            if (transporter == null) return Json(new { success = false, message = "الناقل غير موجود." });

            // حماية البيانات: منع حذف ناقل مرتبط بأوامر نقل
            bool hasOrders = await _context.DispatchOrders.AnyAsync(d => d.TransporterId == id);
            if (hasOrders) return Json(new { success = false, message = "لا يمكن حذف هذا الناقل لارتباطه بأوامر نقل سابقة مسجلة." });

            try
            {
                _context.Transporters.Remove(transporter);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        private bool TransporterExists(int id)
        {
            return _context.Transporters.Any(e => e.Id == id);
        }
    }
}