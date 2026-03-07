using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

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
        // تم إضافة معاملات جديدة لاستقبال المستندات الإضافية
        public async Task<IActionResult> Create(Car car, IFormFile? mainImage, IFormFile? auctionInvoice, IFormFile? bankTransfer, IFormFile? carTitle)
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

                if (string.IsNullOrEmpty(car.InternalCode))
                {
                    car.InternalCode = "AUTO-" + DateTime.Now.Year + "-" + new Random().Next(1000, 9999);
                }

                // معالجة صورة السيارة الرئيسية
                if (mainImage != null && mainImage.Length > 0)
                {
                    car.MainImageUrl = await UploadFileAsync(mainImage, "cars");
                }

                _context.Add(car);
                await _context.SaveChangesAsync(); // حفظ السيارة للحصول على رقمها (Id) لربط المرفقات

                // معالجة ورفع المستندات الإضافية في جدول المرفقات
                await SaveAttachmentAsync(car.Id, auctionInvoice, "AuctionInvoice");
                await SaveAttachmentAsync(car.Id, bankTransfer, "BankTransfer");
                await SaveAttachmentAsync(car.Id, carTitle, "Title");

                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // دوال مساعدة لرفع الملفات بسهولة ونظافة
        // ==========================================
        private async Task<string> UploadFileAsync(IFormFile file, string subFolder)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/uploads/{subFolder}/" + uniqueFileName;
        }

        private async Task SaveAttachmentAsync(int carId, IFormFile? file, string docType)
        {
            if (file != null && file.Length > 0)
            {
                string filePath = await UploadFileAsync(file, "documents");
                var attachment = new DocumentAttachment
                {
                    CarId = carId,
                    DocumentType = docType,
                    FileName = file.FileName,
                    FilePath = filePath,
                    UploadDate = DateTime.Now
                };
                _context.DocumentAttachments.Add(attachment);
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // باقي الدوال (Edit, Delete) كما هي ...
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
                    if (mainImage != null && mainImage.Length > 0)
                    {
                        car.MainImageUrl = await UploadFileAsync(mainImage, "cars");
                    }
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cars.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return Json(new { success = false, message = "السيارة غير موجودة." });

            try
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "لا يمكن حذف السيارة لارتباطها ببيانات أخرى (شحنات، فواتير)." });
            }
        }
    }
}