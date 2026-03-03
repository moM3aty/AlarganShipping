using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env; // لرفع الملفات

        public CarsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
            return View(cars);
        }

        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name");
            ViewBag.AuctionId = new SelectList(_context.Auctions, "Id", "Name");

            ViewBag.TaxTypes = new SelectList(new List<string> { "ضريبة قيمة مضافة", "ضريبة جمركية", "رسوم ميناء" });
            ViewBag.TaxMethods = new SelectList(new List<string> { "نسبة مئوية (%)", "مبلغ مقطوع ($)" });
            ViewBag.VehicleShapes = new SelectList(new List<string> { "صالون (Sedan)", "دفع رباعي (SUV)", "بيك أب (Pickup)" });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car, IFormFile? mainImage)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Auction");
            ModelState.Remove("Shipment");
            ModelState.Remove("TrackingLogs");
            ModelState.Remove("Invoices");
            ModelState.Remove("DocumentAttachments");
            ModelState.Remove("CarInspections");

            if (ModelState.IsValid)
            {
                car.AddDate = DateTime.Now;

                // 1. توليد كود السيارة الداخلي التلقائي
                if (string.IsNullOrEmpty(car.InternalCode))
                {
                    car.InternalCode = "AUTO-" + DateTime.Now.Year + "-" + new Random().Next(1000, 9999);
                }

                // 2. معالجة ورفع الصورة
                if (mainImage != null && mainImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cars");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(mainImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await mainImage.CopyToAsync(fileStream);
                    }
                    car.MainImageUrl = "/uploads/cars/" + uniqueFileName;
                }

                _context.Add(car);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // 1. عرض شاشة التعديل (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", car.CustomerId);
            ViewBag.AuctionId = new SelectList(_context.Auctions, "Id", "Name", car.AuctionId);

            ViewBag.TaxTypes = new SelectList(new List<string> { "ضريبة قيمة مضافة", "ضريبة جمركية", "رسوم ميناء" });
            ViewBag.TaxMethods = new SelectList(new List<string> { "نسبة مئوية (%)", "مبلغ مقطوع ($)" });
            ViewBag.VehicleShapes = new SelectList(new List<string> { "صالون (Sedan)", "دفع رباعي (SUV)", "بيك أب (Pickup)" });

            return View(car);
        }

        // ==========================================
        // 2. حفظ التعديلات في قاعدة البيانات (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, IFormFile? mainImage)
        {
            if (id != car.Id) return NotFound();

            ModelState.Remove("Customer");
            ModelState.Remove("Auction");
            ModelState.Remove("Shipment");
            ModelState.Remove("TrackingLogs");
            ModelState.Remove("Invoices");
            ModelState.Remove("DocumentAttachments");
            ModelState.Remove("CarInspections");

            if (ModelState.IsValid)
            {
                try
                {
                    // إذا تم رفع صورة جديدة، قم بمعالجتها وتحديث المسار
                    if (mainImage != null && mainImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cars");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(mainImage.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await mainImage.CopyToAsync(fileStream);
                        }
                        car.MainImageUrl = "/uploads/cars/" + uniqueFileName;
                    }
                    // ملاحظة: إذا لم يرفع صورة جديدة، سيتم الاحتفاظ بالصورة القديمة لأننا نمررها كـ Hidden Field في الفورم

                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id)) return NotFound();
                    else throw;
                }
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // تحديث جلب البيانات ليشمل التقارير والمستندات
            var car = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Auction)
                .Include(c => c.TrackingLogs)
                .Include(c => c.CarInspections)
                .Include(c => c.DocumentAttachments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();
            return View(car);
        }

        // ==========================================
        // حذف السيارة (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return Json(new { success = false, message = "السيارة غير موجودة." });
            }

            try
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // في حال وجود ارتباطات مثل فواتير أو شحنات تمنع الحذف
                return Json(new { success = false, message = "لا يمكن حذف السيارة لارتباطها ببيانات أخرى (شحنات، فواتير)." });
            }
        }
    }
}