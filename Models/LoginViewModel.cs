using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// ViewModel chứa các trường dữ liệu phục vụ đăng nhập tài khoản người dùng.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Email đăng nhập của tài khoản.
        /// </summary>
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đăng nhập.
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Tuỳ chọn ghi nhớ phiên đăng nhập trên trình duyệt.
        /// </summary>
        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}
