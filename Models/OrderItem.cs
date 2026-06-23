using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Thực thể lưu trữ thông tin Chi tiết mặt hàng (Sản phẩm) trong đơn hàng.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Mã định danh chi tiết đơn hàng.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Khóa ngoại liên kết tới thực thể Đơn hàng.
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Mã định danh sản phẩm thực tế (dùng tham chiếu).
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Lưu snapshot tên sản phẩm tại thời điểm thanh toán.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Lưu snapshot đơn giá sản phẩm tại thời điểm thanh toán.
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng mua đối với sản phẩm này.
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Ảnh hiển thị của sản phẩm lúc đặt mua.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Thuộc tính điều hướng liên kết ngược tới đơn hàng.
        /// </summary>
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
