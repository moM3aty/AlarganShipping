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
    }
}