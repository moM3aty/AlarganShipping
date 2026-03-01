using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AlarganShipping.Models;

namespace AlarganShipping.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض صفحة الإشعارات
        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Customer)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // دالة يتم استدعاؤها عبر AJAX لتحديد الإشعار كمقروء
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // دالة لتحديد جميع الإشعارات كمقروءة
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