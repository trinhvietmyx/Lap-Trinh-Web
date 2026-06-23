using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GIUA_KY_WEB_3AE.Services
{
    /// <summary>
    /// Dịch vụ gửi email sử dụng máy chủ Gmail SMTP.
    /// Lớp này triển khai giao diện IEmailService và tự động lấy thông tin đăng nhập từ cấu hình hệ thống.
    /// </summary>
    public class GmailEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Hàm khởi tạo GmailEmailService.
        /// </summary>
        /// <param name="configuration">Đối tượng cấu hình hệ thống (IConfiguration) dùng để đọc EmailSettings.</param>
        public GmailEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Thực hiện gửi email bất đồng bộ bằng thư viện System.Net.Mail qua máy chủ Gmail SMTP.
        /// </summary>
        /// <param name="toEmail">Địa chỉ email người nhận.</param>
        /// <param name="subject">Tiêu đề email.</param>
        /// <param name="htmlMessage">Nội dung email định dạng HTML.</param>
        /// <returns>Tác vụ bất đồng bộ.</returns>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            // Đọc thông tin cấu hình từ file appsettings.json
            var mailServer = _configuration["EmailSettings:MailServer"] ?? "smtp.gmail.com";
            var mailPort = int.Parse(_configuration["EmailSettings:MailPort"] ?? "587");
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Mì 3AE";
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var password = _configuration["EmailSettings:Password"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"] ?? "true");

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                System.IO.File.AppendAllText("email_log.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CẤU HÌNH THIẾU: SenderEmail='{senderEmail}', Password length={(password != null ? password.Length.ToString() : "null")}\n");
                // Nếu chưa cấu hình email hoặc mật khẩu, ghi log cảnh báo và bỏ qua việc gửi thực tế để tránh crash ứng dụng
                Console.WriteLine("CẢNH BÁO: Chưa cấu hình Email hoặc Password SMTP trong appsettings.json. Bỏ qua việc gửi email.");
                return;
            }

            // Log password thực tế mà IConfiguration đang đọc (che một phần để debug)
            var passwordPreview = password.Length > 4 ? password.Substring(0, 4) + "..." : "****";
            System.IO.File.AppendAllText("email_log.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CẤU HÌNH ĐỌC ĐƯỢC: MailServer='{mailServer}', Port={mailPort}, SenderEmail='{senderEmail}', Password='{passwordPreview}' (length={password.Length}), EnableSSL={enableSsl}\n");

            // Mật khẩu email được đọc trực tiếp từ cấu hình của hệ thống (IConfiguration) để bảo mật.

            // Tạo đối tượng thư điện tử
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(toEmail));

            // Cấu hình máy chủ SMTP của Gmail
            using (var smtpClient = new SmtpClient(mailServer, mailPort))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderEmail, password);
                smtpClient.EnableSsl = enableSsl;

                // Thực hiện gửi email bất đồng bộ
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
