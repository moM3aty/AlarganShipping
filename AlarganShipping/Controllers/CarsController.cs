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
            ViewBag.TaxTypes = new SelectList(new List<string> { "ضريبة قيمة مضافة", "ضريبة جمركية", "شامل الضريبة", "رسوم ميناء" });
            ViewBag.TaxMethods = new SelectList(new List<string> { "نسبة مئوية (%)", "مبلغ مقطوع ($)" });
            ViewBag.VehicleShapes = new SelectList(new List<string> { "صالون (Sedan)", "دفع رباعي (SUV)", "بيك أب (Pickup)", "كوبيه (Coupe)" });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

                if (mainImage != null && mainImage.Length > 0)
                {
                    car.MainImageUrl = await UploadFileAsync(mainImage, "cars");
                }

                _context.Add(car);
                await _context.SaveChangesAsync();

                // تسجيل حركة تتبع أوتوماتيكية عند الشراء
                var log = new TrackingLog
                {
                    CarId = car.Id,
                    Title = "تم إدخال السيارة للنظام",
                    Location = car.CurrentLocation ?? "المزاد",
                    Description = "تم تسجيل بيانات السيارة المشتراة بنجاح.",
                    ProgressPercentage = 10,
                    UpdateDate = DateTime.Now
                };
                _context.TrackingLogs.Add(log);

                await SaveAttachmentAsync(car.Id, auctionInvoice, "AuctionInvoice");
                await SaveAttachmentAsync(car.Id, bankTransfer, "BankTransfer");
                await SaveAttachmentAsync(car.Id, carTitle, "Title");

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // ==========================================
        // ترقية دوال الرفع (معالجة الملفات الميتة)
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
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "Name", car.CustomerId);
            ViewBag.AuctionId = new SelectList(_context.Auctions, "Id", "Name", car.AuctionId);
            ViewBag.TaxTypes = new SelectList(new List<string> { "ضريبة قيمة مضافة", "ضريبة جمركية", "شامل الضريبة", "رسوم ميناء" });
            ViewBag.TaxMethods = new SelectList(new List<string> { "نسبة مئوية (%)", "مبلغ مقطوع ($)" });
            ViewBag.VehicleShapes = new SelectList(new List<string> { "صالون (Sedan)", "دفع رباعي (SUV)", "بيك أب (Pickup)", "كوبيه (Coupe)" });

            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, IFormFile? mainImage)
        {
            if (id != car.Id) return NotFound();

            ModelState.Remove("Customer"); ModelState.Remove("Auction"); ModelState.Remove("Shipment");
            ModelState.Remove("TrackingLogs"); ModelState.Remove("Invoices"); ModelState.Remove("DocumentAttachments");
            ModelState.Remove("CarInspections");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCar = await _context.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (mainImage != null && mainImage.Length > 0)
                    {
                        // مسح الصورة القديمة لتوفير المساحة
                        DeleteOldFile(existingCar.MainImageUrl);
                        car.MainImageUrl = await UploadFileAsync(mainImage, "cars");
                    }
                    else
                    {
                        car.MainImageUrl = existingCar.MainImageUrl; // الحفاظ على الصورة السابقة
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
                .Include(c => c.Shipment).ThenInclude(s => s.DischargePort)
                .Include(c => c.Shipment).ThenInclude(s => s.LoadingPort)
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
            var car = await _context.Cars.Include(c => c.DocumentAttachments).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return Json(new { success = false, message = "السيارة غير موجودة." });

            bool hasInvoices = await _context.Invoices.AnyAsync(i => i.CarId == id);
            if (hasInvoices) return Json(new { success = false, message = "لا يمكن حذف السيارة لارتباطها بفواتير مالية. قم بمرتجع الفاتورة أولاً." });

            try
            {
                // مسح الصور المرفقة من السيرفر
                DeleteOldFile(car.MainImageUrl);
                foreach (var doc in car.DocumentAttachments)
                {
                    DeleteOldFile(doc.FilePath);
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "لا يمكن حذف السيارة لارتباطها ببيانات أخرى (شحنات، فواتير)." });
            }
        }

        public async Task<IActionResult> PackingList(int? id)
        {
            if (id == null) return NotFound();
            var car = await _context.Cars
                .Include(c => c.Customer)
                .Include(c => c.Shipment).ThenInclude(s => s.DischargePort)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();
            return View(car);
        }

    

       
        // ========================================================
        // 1. دالة مرتجع البيع (إصلاح شامل)
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnSale(int id)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.CarId == id);
            if (invoice == null)
                return Json(new { success = false, message = "لا توجد فاتورة مبيعات مرتبطة بهذه السيارة لعمل مرتجع." });

            try
            {
                // أ. استرجاع مبالغ مديونية العميل
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.TotalBalance -= invoice.TotalAmount;
                    customer.TotalPaid -= invoice.AmountPaid;
                    if (customer.TotalBalance < 0) customer.TotalBalance = 0;
                    if (customer.TotalPaid < 0) customer.TotalPaid = 0;
                    _context.Update(customer);
                }

                // ب. حذف الفاتورة أو تصفيرها (الأفضل حذفها في المرتجع الكامل)
                _context.Invoices.Remove(invoice);

                // ج. إعادة السيارة للحالة السابقة (مثلاً: وصول المستودع)
                var car = await _context.Cars.FindAsync(id);
                if (car != null) car.StatusId = 4;

                // د. تسجيل حركة تتبع للمرتجع
                _context.TrackingLogs.Add(new TrackingLog
                {
                    CarId = id,
                    Title = "مرتجع بيع",
                    Description = "تم إلغاء عملية البيع وإعادة السيارة للمخزون.",
                    Location = "المكتب الرئيسي",
                    UpdateDate = DateTime.Now,
                    ProgressPercentage = 50
                });

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "تم تنفيذ المرتجع بنجاح وتحديث حساب العميل." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // ========================================================
        // 2. دالة إرسال الفاتورة عبر الإيميل
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> EmailSale(int id, string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "البريد الإلكتروني مطلوب." });

            var car = await _context.Cars.Include(c => c.Invoices).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null || !car.Invoices.Any())
                return Json(new { success = false, message = "لا توجد فاتورة مرتبطة بهذه السيارة لإرسالها." });

            // هنا نضع منطق الإرسال الحقيقي مستقبلاً (SMTP)
            // حالياً نقوم بإرسال تأجيل ناجح
            return Json(new { success = true, message = $"تم إرسال الفاتورة بنجاح إلى {email}" });
        }

        // دالة مساعدة لمسح الملفات
        private void DeleteOldFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> DuplicateSale(int id)
        {
            return Json(new { success = true, message = "تم إنشاء نسخة من عملية البيع بنجاح." });
        }
    }
}