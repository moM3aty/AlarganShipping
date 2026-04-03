using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

namespace AlarganShipping.Controllers
{
    [Authorize]
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

        // ========================================================
        // 💡 تعديل جوهري: استقبال قائمة من الملفات (List<IFormFile>)
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentAttachment documentAttachment, List<IFormFile> uploadedFiles)
        {
            ModelState.Remove("Car");
            ModelState.Remove("Customer");
            ModelState.Remove("Shipment");
            ModelState.Remove("FileName");
            ModelState.Remove("FilePath");

            // التأكد من أن الموظف قام بتحديد ملف واحد على الأقل
            if (ModelState.IsValid && uploadedFiles != null && uploadedFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 💡 الدوران (Loop) على كل الملفات المرفوعة وحفظها
                foreach (var file in uploadedFiles)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // 💡 إنشاء كائن (Object) جديد لكل ملف لكي يتم إضافته في قاعدة البيانات كصف مستقل
                        var newAttachment = new DocumentAttachment
                        {
                            CarId = documentAttachment.CarId,
                            CustomerId = documentAttachment.CustomerId,
                            ShipmentId = documentAttachment.ShipmentId,
                            DocumentType = documentAttachment.DocumentType, // مثلا: "CarImage"
                            Description = documentAttachment.Description,

                            // بيانات الملف الفعلي
                            FileName = file.FileName,
                            FilePath = "/uploads/" + uniqueFileName,
                            UploadDate = DateTime.Now
                        };

                        _context.Add(newAttachment);
                    }
                }

                // حفظ كل الملفات في قاعدة البيانات دفعة واحدة
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = new[] { "يرجى التأكد من إرفاق صورة/ملف واحد على الأقل." } });
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