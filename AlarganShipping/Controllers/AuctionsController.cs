using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuctionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة المزادات
        public async Task<IActionResult> Index()
        {
            return View(await _context.Auctions.ToListAsync());
        }

        // شاشة إضافة مزاد (GET)
        public IActionResult Create()
        {
            return View();
        }

        // حفظ مزاد جديد (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Auction auction)
        {
            ModelState.Remove("Cars");

            if (ModelState.IsValid)
            {
                _context.Add(auction);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // شاشة تعديل مزاد (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();

            return View(auction);
        }

        // حفظ التعديلات (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Auction auction)
        {
            if (id != auction.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });

            ModelState.Remove("Cars");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auction);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionExists(auction.Id)) return Json(new { success = false, errors = new[] { "المزاد غير موجود." } });
                    else throw;
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // حذف مزاد (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return Json(new { success = false, message = "المزاد غير موجود." });
            }

            // منع حذف المزاد إذا كانت هناك سيارات مرتبطة به
            bool hasCars = await _context.Cars.AnyAsync(c => c.AuctionId == id);
            if (hasCars)
            {
                return Json(new { success = false, message = "لا يمكن حذف هذا المزاد لوجود سيارات مسجلة فيه. قم بنقلها أو حذفها أولاً." });
            }

            try
            {
                _context.Auctions.Remove(auction);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }

        private bool AuctionExists(int id)
        {
            return _context.Auctions.Any(e => e.Id == id);
        }
    }
}