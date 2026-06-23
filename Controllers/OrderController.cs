using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GIUA_KY_WEB_3AE.Controllers
{
    /// <summary>
    /// Controller chịu trách nhiệm quản lý Đơn hàng và Lịch sử mua hàng của người dùng.
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Hàm khởi tạo OrderController.
        /// </summary>
        public OrderController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        /// <summary>
        /// Action hiển thị danh sách Lịch sử đơn hàng của người dùng đang đăng nhập (GET).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Lấy danh sách các đơn hàng của user, sắp xếp theo thứ tự mới nhất
            var orders = await _dbContext.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        /// <summary>
        /// Action hiển thị thông tin Chi tiết đơn hàng cụ thể (GET).
        /// Chỉ cho phép chính chủ sở hữu đơn hàng (hoặc Admin) xem chi tiết đơn hàng đó.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Tải thông tin đơn hàng kèm theo chi tiết các sản phẩm bên trong
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra bảo mật: Chỉ admin hoặc chính chủ đơn hàng mới có quyền xem
            var isAdmin = User.IsInRole("Admin");
            if (order.UserId != user.Id && !isAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(order);
        }
    }
}
