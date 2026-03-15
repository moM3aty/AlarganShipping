using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;
using Microsoft.AspNetCore.Authorization;

namespace AlarganShipping.Controllers
{
    [Authorize] // يجب أن يكون مسجلاً للدخول
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة الموظفين
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.OrderByDescending(u => u.Id).ToListAsync();
            return View(users);
        }

        // شاشة إضافة موظف
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // التأكد من عدم تكرار الإيميل
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return Json(new { success = false, errors = new[] { "البريد الإلكتروني مستخدم بالفعل لموظف آخر." } });
                }

                user.CreatedAt = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // شاشة التعديل
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // تصفير الباسورد في الـ View لأسباب أمنية (اختياري)
            // user.PasswordHash = ""; 

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id) return Json(new { success = false, errors = new[] { "خطأ في المعرف." } });
            ModelState.Remove("PasswordHash");
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser != null)
                    {
                        user.CreatedAt = existingUser.CreatedAt;

                        // إذا لم يقم بكتابة باسورد جديد، نحتفظ بالقديم
                        if (string.IsNullOrEmpty(user.PasswordHash))
                        {
                            user.PasswordHash = existingUser.PasswordHash;
                        }
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "حدث خطأ: " + ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // حذف موظف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // منع حذف آخر أدمن في النظام
                if (user.Role == "Admin" && await _context.Users.CountAsync(u => u.Role == "Admin") <= 1)
                {
                    return Json(new { success = false, message = "عذراً، لا يمكن حذف المسؤول (Admin) الوحيد في النظام!" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "الموظف غير موجود." });
        }
    }
}