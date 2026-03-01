using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    // متحكم المرفقات والأرشفة الإلكترونية (رفع الملفات)
    [Authorize]
    public class DocumentAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentAttachmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // شاشة رفع ملف جديد
        public IActionResult Create(int? carId)
        {
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", carId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentAttachment document, IFormFile uploadedFile)
        {
            if (ModelState.IsValid)
            {
                // عملية معالجة وحفظ الملف المرفوع في مجلد wwwroot/uploads
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    // تأكد من وجود مجلد uploads
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    // توليد اسم ملف فريد لتجنب التكرار
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(stream);
                    }

                    document.FilePath = "/uploads/" + uniqueFileName;
                    document.FileName = uploadedFile.FileName;
                    document.UploadDate = DateTime.Now;

                    _context.Add(document);
                    await _context.SaveChangesAsync();

                    if (document.CarId.HasValue)
                        return RedirectToAction("Details", "Cars", new { id = document.CarId });

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "الرجاء اختيار ملف لرفعه.");
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "VIN", document.CarId);
            return View(document);
        }
    }
}