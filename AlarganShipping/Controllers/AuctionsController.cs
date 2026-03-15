using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

namespace AlarganShipping.Controllers
{
    [Authorize]
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
            return View(await _context.Auctions.OrderByDescending(a => a.Id).ToListAsync());
        }

        // شاشة الإضافة (عادية)
        public IActionResult Create()
        {
            return View();
        }

        // 🌟 دالة الإضافة (تعمل مع الشاشة العادية وتعمل مع النافذة المنبثقة من السيارات) 🌟
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Auction auction)
        {
            // 1. حماية الـ NULL: إذا كان رقم المشتري أو الموقع فارغاً، نعطيهم قيمة افتراضية
            if (string.IsNullOrWhiteSpace(auction.BuyerNumber))
            {
                auction.BuyerNumber = "غير محدد";
                // الأهم: إزالة الخطأ من نظام الفحص لكي لا يتوقف الكود
                ModelState.Remove("BuyerNumber");
            }

            if (string.IsNullOrWhiteSpace(auction.Location))
            {
                auction.Location = "غير محدد";
                ModelState.Remove("Location");
            }
            ModelState.Remove("Cars");
            // 2. فحص حالة البيانات وحفظها
            if (ModelState.IsValid)
            {
                _context.Add(auction);
                await _context.SaveChangesAsync();

                // 3. إذا كان الطلب قادم من النافذة المنبثقة (AJAX) في شاشة السيارات
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, id = auction.Id, name = auction.Name });
                }

                // 4. إذا كان من شاشة الإضافة العادية
                return RedirectToAction(nameof(Index));
            }

            // في حال الفشل عبر الـ AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "تأكد من إدخال البيانات بشكل صحيح." });
            }

            return View(auction);
        }

        // 🌟 دالة الإضافة السريعة (بديلة في حال استخدمت الجافاسكربت مساراً مختلفاً) 🌟
        [HttpPost]
        public async Task<IActionResult> CreateQuick(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "اسم المزاد مطلوب." });
            }

            try
            {
                var auction = new Auction
                {
                    Name = name,
                    BuyerNumber = "غير محدد", // منع خطأ الـ NULL
                    Location = "غير محدد",
                    DefaultAuctionFee = 0
                };

                _context.Auctions.Add(auction);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = auction.Id, name = auction.Name });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // شاشة التعديل
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();
            return View(auction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Auction auction)
        {
            if (id != auction.Id) return NotFound();

            if (string.IsNullOrWhiteSpace(auction.BuyerNumber))
            {
                auction.BuyerNumber = "غير محدد";
                ModelState.Remove("BuyerNumber");
            }
            if (string.IsNullOrWhiteSpace(auction.Location))
            {
                auction.Location = "غير محدد";
                ModelState.Remove("Location");
            }
            ModelState.Remove("Cars");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionExists(auction.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(auction);
        }

        // الحذف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction != null)
            {
                // لا تسمح بحذف مزاد به سيارات (اختياري، يفضل استخدامه)
                var hasCars = await _context.Cars.AnyAsync(c => c.AuctionId == id);
                if (hasCars) return Json(new { success = false, message = "لا يمكن حذف هذا المزاد لوجود سيارات مسجلة باسمه." });

                _context.Auctions.Remove(auction);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "المزاد غير موجود." });
        }

        private bool AuctionExists(int id)
        {
            return _context.Auctions.Any(e => e.Id == id);
        }
    }
}