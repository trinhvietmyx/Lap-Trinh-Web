using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Giao dịch thu chi (Transaction).
    /// </summary>
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Năm")]
        public int Year { get; set; }

        [Display(Name = "Tháng")]
        public int Month { get; set; }

        [Display(Name = "Ngày")]
        public int Day { get; set; }

        [Required(ErrorMessage = "Loại giao dịch là bắt buộc.")]
        [StringLength(20)]
        [Display(Name = "THU/CHI")]
        public string Type { get; set; } = string.Empty; // "THU" hoặc "CHI"

        [Required(ErrorMessage = "Nội dung giao dịch là bắt buộc.")]
        [StringLength(500)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [Display(Name = "Lượng đơn")]
        public int OrderCount { get; set; }

        [StringLength(100)]
        [Display(Name = "Phân loại/Kênh")]
        public string? Category { get; set; } // "Khoản chi", "Khách Ngoài", "Be Food", "Grab Food", etc.

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
