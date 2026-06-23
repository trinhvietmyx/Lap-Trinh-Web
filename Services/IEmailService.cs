using System.Threading.Tasks;

namespace GIUA_KY_WEB_3AE.Services
{
    /// <summary>
    /// Giao diện định nghĩa dịch vụ gửi email trong hệ thống.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email bất đồng bộ đến địa chỉ nhận được chỉ định.
        /// </summary>
        /// <param name="toEmail">Địa chỉ email của người nhận.</param>
        /// <param name="subject">Tiêu đề của email.</param>
        /// <param name="htmlMessage">Nội dung email định dạng HTML.</param>
        /// <returns>Một tác vụ bất đồng bộ đại diện cho quá trình gửi email.</returns>
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
