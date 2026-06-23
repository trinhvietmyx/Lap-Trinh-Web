using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Nguyên liệu (Ingredient).
    /// </summary>
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nguyên liệu là bắt buộc.")]
        [StringLength(150)]
        [Display(Name = "Tên nguyên liệu")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Quy cách đóng gói")]
        public string? Specification { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập")]
        public decimal ImportPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Số lượng trong gói")]
        public decimal PackageQuantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Đơn giá lẻ")]
        public decimal UnitPrice { get; set; }
    }
}
