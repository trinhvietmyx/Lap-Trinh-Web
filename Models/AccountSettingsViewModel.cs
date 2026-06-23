using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// ViewModel phục vụ biểu mẫu Cài đặt tài khoản (cập nhật thông tin cá nhân).
    /// </summary>
    public class AccountSettingsViewModel
    {
        /// <summary>
        /// Họ và tên đầy đủ của người dùng.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên hệ cá nhân.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ giao hàng mặc định của người dùng.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ liên hệ/giao hàng của bạn.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;
    }
}
