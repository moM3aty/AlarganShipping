using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Customer)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(notifications);
        }

        // دالة يتم استدعاؤها من خلال AJAX في الـ Layout لجلب عدد الإشعارات
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _context.Notifications.CountAsync(n => !n.IsRead);
            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notif = await _context.Notifications.FindAsync(id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unreadNotifs = await _context.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var notif in unreadNotifs)
            {
                notif.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}