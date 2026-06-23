using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Thực thể lưu trữ thông tin Đơn hàng trong cơ sở dữ liệu.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Mã định danh duy nhất của đơn hàng.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh của tài khoản người dùng thực hiện đặt hàng.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Họ tên của người nhận hàng.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ tên người nhận")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên hệ nhận hàng.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ nhận hàng chi tiết.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
        [StringLength(250, ErrorMessage = "Địa chỉ nhận hàng không được vượt quá 250 ký tự.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú từ khách hàng cho đơn vị vận chuyển.
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        [Display(Name = "Ghi chú thêm")]
        public string? Notes { get; set; }

        /// <summary>
        /// Tổng giá trị tiền tệ của cả đơn hàng.
        /// </summary>
        [Required]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Ngày giờ đơn hàng được khởi tạo.
        /// </summary>
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Trạng thái thanh toán/vận chuyển của đơn hàng (mặc định: Pending).
        /// </summary>
        [Required]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Bộ sưu tập các chi tiết mặt hàng đi kèm đơn hàng này.
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
