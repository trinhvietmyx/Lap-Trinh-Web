using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Danh mục sản phẩm (Category).
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Mã định danh duy nhất cho danh mục.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Tên của danh mục món ăn (ví dụ: Mì Quảng, Món Ăn Kèm, Nước Uống...).
        /// </summary>
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên danh mục không được vượt quá 50 ký tự.")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;
    }
}
