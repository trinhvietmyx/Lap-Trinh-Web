using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using GIUA_KY_WEB_3AE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GIUA_KY_WEB_3AE.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý Đơn hàng (Orders) trong khu vực Admin (Area Admin).
    /// Hỗ trợ Admin theo dõi tất cả đơn hàng trên hệ thống, cập nhật trạng thái đơn hàng và xem chi tiết.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Hàm khởi tạo OrderController của Admin.
        /// </summary>
        public OrderController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Action hiển thị danh sách tất cả đơn hàng cho Admin quản lý (GET).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var orders = await _dbContext.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        /// <summary>
        /// Action hiển thị thông tin chi tiết đơn hàng dành cho Admin (GET).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Action xử lý việc cập nhật trạng thái đơn hàng của Admin (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? cancellationReason)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Manage));
            }

            var validStatuses = new List<string> { "Pending", "PendingPayment", "Confirmed", "Shipped", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Trạng thái đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Manage));
            }

            var oldStatus = order.Status;
            if (string.Equals(oldStatus, status, StringComparison.OrdinalIgnoreCase))
            {
                TempData["SuccessMessage"] = $"Đơn hàng #{order.Id} đang ở trạng thái này.";
                return RedirectToAction(nameof(Manage));
            }

            if (status == "Cancelled" && !string.IsNullOrEmpty(cancellationReason))
            {
                order.Notes = string.IsNullOrEmpty(order.Notes)
                    ? $"[Lý do hủy: {cancellationReason}]"
                    : $"{order.Notes}, [Lý do hủy: {cancellationReason}]";
            }

            order.Status = status;
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
            await _notificationService.CreateOrderStatusNotificationAsync(order, oldStatus, status, reason: cancellationReason);

            string vietnameseStatus = status switch
            {
                "Pending" => "Chờ duyệt",
                "PendingPayment" => "Chờ xác nhận thanh toán",
                "Confirmed" => "Đã xác nhận",
                "Shipped" => "Đang giao hàng",
                "Completed" => "Đã hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => status
            };

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.Id} thành '{vietnameseStatus}' thành công!";
            return RedirectToAction(nameof(Manage));
        }
    }
}
