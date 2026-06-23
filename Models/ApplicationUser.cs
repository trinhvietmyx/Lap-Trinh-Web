using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp biểu diễn người dùng tùy chỉnh mở rộng từ IdentityUser.
    /// Cho phép lưu trữ thêm các thông tin cá nhân như Họ tên, Địa chỉ, Tuổi.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Họ và tên đầy đủ của người dùng.
        /// </summary>
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ liên hệ của người dùng.
        /// </summary>
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string? Address { get; set; }

        /// <summary>
        /// Tuổi của người dùng.
        /// </summary>
        [StringLength(10, ErrorMessage = "Tuổi không hợp lệ.")]
        public string? Age { get; set; }
    }
}
