using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Sản phẩm (Product) với các thông tin chi tiết và validation dữ liệu.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Mã định danh duy nhất của sản phẩm.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Tên sản phẩm.
        /// </summary>
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Giá bán sản phẩm.
        /// </summary>
        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc.")]
        [Range(0.01, 1000000000.0, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        /// <summary>
        /// Mô tả chi tiết về sản phẩm (có thể null).
        /// </summary>
        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        /// <summary>
        /// Đường dẫn hình ảnh chính của sản phẩm (có thể null).
        /// </summary>
        [Display(Name = "Hình ảnh chính")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Danh sách các hình ảnh phụ liên kết 1-Nhiều từ bảng ProductImages.
        /// </summary>
        public List<ProductImage>? Images { get; set; }

        /// <summary>
        /// Danh sách đường dẫn các hình ảnh phụ (không lưu trực tiếp vào bảng Products).
        /// Tự động đồng bộ hóa với thuộc tính Images để giữ nguyên sự tương thích với code cũ.
        /// </summary>
        [NotMapped]
        [Display(Name = "Danh sách hình ảnh phụ")]
        public List<string>? ImageUrls
        {
            get => Images?.Select(img => img.Url).ToList() ?? new List<string>();
            set
            {
                Images = value?.Select(url => new ProductImage { Url = url }).ToList() ?? new List<ProductImage>();
            }
        }

        /// <summary>
        /// Mã danh mục mà sản phẩm này thuộc về.
        /// </summary>
        [Required(ErrorMessage = "Danh mục sản phẩm là bắt buộc.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
    }
}
