using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;

namespace GIUA_KY_WEB_3AE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateOrderStatusNotificationAsync(Order order, string? oldStatus, string newStatus, bool isInitial = false, string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(order.UserId))
            {
                return;
            }

            if (!isInitial && string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var statusLabel = GetStatusLabel(newStatus);
            var title = isInitial
                ? $"Đơn hàng #{order.Id} đã được tạo"
                : $"Đơn hàng #{order.Id}: {statusLabel}";

            var notification = new Notification
            {
                UserId = order.UserId,
                OrderId = order.Id,
                OrderStatus = newStatus,
                Title = title,
                Message = BuildMessage(order.Id, newStatus, statusLabel, isInitial, reason),
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();
        }

        private static string BuildMessage(int orderId, string status, string statusLabel, bool isInitial, string? reason = null)
        {
            if (isInitial)
            {
                return $"Đơn hàng #{orderId} của bạn đã được ghi nhận và đang ở trạng thái {statusLabel}.";
            }

            return status switch
            {
                "Pending" => $"Đơn hàng #{orderId} đang chờ quán duyệt. Mì 3AE sẽ kiểm tra và phản hồi sớm.",
                "PendingPayment" => $"Đơn hàng #{orderId} đang chờ xác nhận thanh toán. Quán sẽ duyệt sau khi kiểm tra giao dịch.",
                "Confirmed" => $"Đơn hàng #{orderId} đã được xác nhận. Bếp đang chuẩn bị món cho bạn.",
                "Shipped" => $"Đơn hàng #{orderId} đang được giao. Bạn vui lòng để ý điện thoại để nhận món.",
                "Completed" => $"Đơn hàng #{orderId} đã hoàn thành. Cảm ơn bạn đã đặt món tại Mì 3AE.",
                "Cancelled" => string.IsNullOrEmpty(reason)
                    ? $"Đơn hàng #{orderId} đã bị hủy. Bạn có thể xem chi tiết đơn hoặc đặt lại món khác."
                    : $"Đơn hàng #{orderId} đã bị hủy. Lý do: {reason}.",
                _ => $"Đơn hàng #{orderId} đã chuyển sang trạng thái {statusLabel}."
            };
        }

        private static string GetStatusLabel(string status)
        {
            return status switch
            {
                "Pending" => "Chờ duyệt",
                "PendingPayment" => "Chờ xác nhận thanh toán",
                "Confirmed" => "Đã xác nhận",
                "Shipped" => "Đang giao hàng",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => status
            };
        }
    }
}
