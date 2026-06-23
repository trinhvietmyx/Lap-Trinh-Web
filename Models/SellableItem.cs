using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Món bán (SellableItem).
    /// </summary>
    public class SellableItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món bán là bắt buộc.")]
        [StringLength(150)]
        [Display(Name = "Tên món bán")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá vốn ước tính")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Lợi nhuận ước tính")]
        public decimal EstimatedProfit { get; set; }
    }
}
