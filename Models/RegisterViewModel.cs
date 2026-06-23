using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// ViewModel chứa các trường thông tin phục vụ cho biểu mẫu Đăng ký tài khoản thành viên mới.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Họ của người đăng ký.
        /// </summary>
        [Required(ErrorMessage = "Họ là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự.")]
        [Display(Name = "Họ")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Tên của người đăng ký.
        /// </summary>
        [Required(ErrorMessage = "Tên là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ Email của người đăng ký.
        /// </summary>
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đăng ký của tài khoản.
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Xác nhận lại mật khẩu để tránh gõ sai.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
