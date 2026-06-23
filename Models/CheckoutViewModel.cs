using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// ViewModel phục vụ biểu mẫu nhập thông tin giao hàng tại trang Checkout.
    /// </summary>
    public class CheckoutViewModel
    {
        /// <summary>
        /// Họ tên của người nhận.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ tên người nhận")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên hệ giao hàng.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ giao nhận hàng.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
        [StringLength(250, ErrorMessage = "Địa chỉ nhận hàng không được vượt quá 250 ký tự.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú đơn hàng nếu có.
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        [Display(Name = "Ghi chú thêm")]
        public string? Notes { get; set; }

        /// <summary>
        /// Danh sách các mặt hàng trong giỏ hàng hiện tại (dùng để hiển thị tóm tắt hóa đơn).
        /// </summary>
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        /// <summary>
        /// Tổng giá trị đơn hàng.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Danh sách ID của các sản phẩm được chọn để thanh toán.
        /// </summary>
        public List<int> SelectedIds { get; set; } = new List<int>();

        /// <summary>
        /// Cho biết người dùng có sử dụng điểm thành viên Mì 3AE để giảm giá hay không.
        /// Nếu true, server sẽ trừ 22.190đ vào tổng tiền đơn hàng.
        /// </summary>
        [Display(Name = "Sử dụng điểm thành viên")]
        public bool UseMemberPoints { get; set; } = false;

        /// <summary>
        /// Phí vận chuyển được tính toán từ Client (GPS hoặc tỉnh thành).
        /// </summary>
        public decimal ShippingFee { get; set; } = 0;
    }
}
