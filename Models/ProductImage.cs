using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng ảnh phụ của sản phẩm (ProductImage).
    /// Thiết lập mối quan hệ 1-Nhiều với bảng Products.
    /// </summary>
    [Table("ProductImages")]
    public class ProductImage
    {
        /// <summary>
        /// Mã định danh duy nhất cho ảnh phụ (Khóa chính).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Đường dẫn lưu trữ hình ảnh phụ.
        /// </summary>
        [Required]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Mã sản phẩm liên kết (Khóa ngoại).
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Thực thể sản phẩm chứa ảnh phụ này.
        /// </summary>
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
