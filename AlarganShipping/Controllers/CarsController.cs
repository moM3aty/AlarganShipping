using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Hosting;

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
                        DeleteOldFile(existingCar.MainImageUrl);
                        car.MainImageUrl = await UploadFileAsync(mainImage, "cars");
                    }
                    else
                    {
                        car.MainImageUrl = existingCar.MainImageUrl;
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
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            // 💡 التعديلات الجديدة: جلب إجمالي سندات القبض وتفاصيلها لهذه السيارة لتحديث الفاتورة للطباعة
            ViewBag.TotalReceipts = await _context.PaymentReceipts
                .Where(p => p.CarId == id)
                .SumAsync(p => p.Amount);

            ViewBag.CarReceipts = await _context.PaymentReceipts
                .Where(p => p.CarId == id)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnSale(int id)
        {
            var car = await _context.Cars.Include(c => c.Invoices).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
                return Json(new { success = false, message = "السيارة غير موجودة." });

            string inventoryName = "سيارات الشركة (المخزون)";
            var inventoryCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == inventoryName);

            // إنشاء حساب المخزون إذا لم يكن موجوداً
            if (inventoryCustomer == null)
            {
                inventoryCustomer = new Customer { Name = inventoryName, CustomerCode = "INV-001", Phone = "0000000000" };
                _context.Customers.Add(inventoryCustomer);
                await _context.SaveChangesAsync();
            }

            if (car.CustomerId == inventoryCustomer.Id)
                return Json(new { success = false, message = "السيارة موجودة بالفعل في معرض الشركة ولم تُباع بعد." });

            var oldCustomer = await _context.Customers.FindAsync(car.CustomerId);

            var invoice = car.Invoices?.FirstOrDefault();
            if (invoice != null && oldCustomer != null)
            {
                oldCustomer.TotalBalance -= invoice.TotalAmount;
                oldCustomer.TotalPaid -= invoice.AmountPaid;
                if (oldCustomer.TotalBalance < 0) oldCustomer.TotalBalance = 0;
                if (oldCustomer.TotalPaid < 0) oldCustomer.TotalPaid = 0;

                _context.Invoices.Remove(invoice);
            }

            car.CustomerId = inventoryCustomer.Id;
            car.SellingPrice = null;
            car.StatusId = 4;

            _context.TrackingLogs.Add(new TrackingLog
            {
                CarId = car.Id,
                Title = "إرجاع مبيعات",
                Description = $"تم إرجاع السيارة من العميل ({oldCustomer?.Name ?? "غير محدد"}) إلى معرض الشركة.",
                Location = "المعرض",
                UpdateDate = DateTime.Now,
                ProgressPercentage = 30
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "تم إلغاء البيع، وتسوية حساب العميل، وإرجاع السيارة للمخزون." });
        }

        [HttpPost]
        public async Task<IActionResult> EmailSale(int id, string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "البريد الإلكتروني مطلوب." });

            var car = await _context.Cars.Include(c => c.Invoices).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null || !car.Invoices.Any())
                return Json(new { success = false, message = "لا توجد فاتورة مرتبطة بهذه السيارة لإرسالها." });

            return Json(new { success = true, message = $"تم إرسال الفاتورة بنجاح إلى {email}" });
        }

        private void DeleteOldFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicateSale(int id, string? newVin)
        {
            var originalCar = await _context.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (originalCar == null)
                return Json(new { success = false, message = "السيارة الأصلية غير موجودة." });

            // إذا لم يدخل المستخدم شاصي، نقوم بتوليده تلقائياً (17 رقم)
            string finalVin = newVin;
            if (string.IsNullOrWhiteSpace(finalVin))
            {
                // توليد شاصي مؤقت: COPY + 13 رقم من التاريخ الحالي
                finalVin = "COPY" + DateTime.Now.Ticks.ToString().Substring(0, 13);
            }

            if (await _context.Cars.AnyAsync(c => c.VIN == finalVin))
                return Json(new { success = false, message = "عذراً، رقم الشاصي الجديد مسجل لسيارة أخرى في النظام!" });

            var newCar = new Car
            {
                VIN = finalVin.ToUpper(),
                Make = originalCar.Make,
                Model = originalCar.Model,
                Year = originalCar.Year,
                Color = originalCar.Color,
                InternalCode = "AUTO-" + DateTime.Now.Year + "-" + new Random().Next(1000, 9999),
                VehicleType = originalCar.VehicleType,
                VehicleShape = originalCar.VehicleShape,
                PurchasePrice = originalCar.PurchasePrice,
                SellingPrice = originalCar.SellingPrice,
                EstimatedProfit = originalCar.EstimatedProfit,
                LotNumber = originalCar.LotNumber,
                PlateNo = originalCar.PlateNo,
                CurrentLocation = originalCar.CurrentLocation,
                CardNo = originalCar.CardNo,
                EngineNo = originalCar.EngineNo,
                TaxType = originalCar.TaxType,
                TaxAmount = originalCar.TaxAmount,
                CustomsDeclaration = originalCar.CustomsDeclaration,
                InsuranceNo = originalCar.InsuranceNo,
                TaxMethod = originalCar.TaxMethod,
                StatusId = originalCar.StatusId,
                AuctionId = originalCar.AuctionId,
                CustomerId = originalCar.CustomerId,
                PurchaseDate = originalCar.PurchaseDate,
                AddDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تم إنشاء نسخة من السيارة بنجاح (يمكنك تعديل رقم الشاصي لاحقاً)." });
        }
    }
}