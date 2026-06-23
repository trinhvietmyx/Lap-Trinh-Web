using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Khách hàng (Customer).
    /// </summary>
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Tên khách hàng")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Phường/Xã")]
        public string? Ward { get; set; }

        [StringLength(100)]
        [Display(Name = "Quận/Huyện")]
        public string? District { get; set; }

        [StringLength(100)]
        [Display(Name = "Khu vực")]
        public string? Area { get; set; }
    }
}
