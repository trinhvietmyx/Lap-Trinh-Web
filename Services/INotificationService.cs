using GIUA_KY_WEB_3AE.Models;

namespace GIUA_KY_WEB_3AE.Services
{
    public interface INotificationService
    {
        Task CreateOrderStatusNotificationAsync(Order order, string? oldStatus, string newStatus, bool isInitial = false, string? reason = null);
    }
}
