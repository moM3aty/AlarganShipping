using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class DocumentAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentAttachmentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; // للوصول لمجلد wwwroot
        }

        public IActionResult Create(int? carId)
        {
            ViewBag.CarId = new SelectList(_context.Cars, "Id", "VIN", carId);
            return View();
        }

        // معالجة رفع الملفات بشكل حقيقي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentAttachment documentAttachment, IFormFile uploadedFile)
        {
            ModelState.Remove("Car");
            ModelState.Remove("Customer");
            ModelState.Remove("Shipment");
            ModelState.Remove("FileName");
            ModelState.Remove("FilePath");

            if (ModelState.IsValid && uploadedFile != null && uploadedFile.Length > 0)
            {
                // التأكد من وجود مجلد الرفع
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // إنشاء اسم فريد للملف لتجنب التكرار
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                documentAttachment.FileName = uploadedFile.FileName;
                documentAttachment.FilePath = "/uploads/" + uniqueFileName;
                documentAttachment.UploadDate = DateTime.Now;

                _context.Add(documentAttachment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = new[] { "يرجى التأكد من إرفاق الملف." } });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. البحث عن المستند في قاعدة البيانات
            var document = await _context.DocumentAttachments.FindAsync(id);

            if (document == null)
            {
                return Json(new { success = false, message = "المستند غير موجود أو تم حذفه مسبقاً." });
            }

            try
            {
                // 2. حذف الملف الفعلي (Physical File) من السيرفر (مجلد wwwroot)
                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    // تنظيف المسار لضمان دمجه بشكل صحيح
                    var fileRelativePath = document.FilePath.TrimStart('~', '/').Replace("/", "\\");
                    var fullPath = Path.Combine(_env.WebRootPath, fileRelativePath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // 3. حذف السجل من قاعدة البيانات
                _context.DocumentAttachments.Remove(document);
                await _context.SaveChangesAsync();

                // 4. إرجاع رسالة نجاح للـ AJAX
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // في حال حدوث مشكلة (مثلاً الملف قيد الاستخدام من برنامج آخر)
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف: " + ex.Message });
            }
        }
    }
}