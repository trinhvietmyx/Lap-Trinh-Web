using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GIUA_KY_WEB_3AE.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var unreadNotifications = await _dbContext.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }

            if (notification.OrderId.HasValue)
            {
                return RedirectToAction("Details", "Order", new { id = notification.OrderId.Value });
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// API lấy danh sách 5 thông báo mới nhất dưới dạng JSON phục vụ cho Dropdown trên Header.
        /// </summary>
        /// <returns>Danh sách thông báo dạng JSON.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    orderId = n.OrderId,
                    orderStatus = n.OrderStatus
                })
                .ToListAsync();

            return Json(notifications);
        }

        /// <summary>
        /// API đánh dấu tất cả các thông báo của người dùng là đã đọc thông qua AJAX.
        /// </summary>
        /// <returns>Trạng thái thành công dạng JSON.</returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> MarkAllAsReadApi()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var unreadNotifications = await _dbContext.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
