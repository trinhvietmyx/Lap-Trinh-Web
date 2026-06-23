using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Dây nợ/Dây hụi (DebtLedger).
    /// </summary>
    public class DebtLedger
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên dây nợ là bắt buộc.")]
        [StringLength(150)]
        [Display(Name = "Tên tài khoản/dây")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Chu kỳ thời gian")]
        public string? PeriodRange { get; set; } // Ví dụ: "16/04/2026-02/07/2026"

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng số tiền")]
        public decimal TotalAmount { get; set; } // Giá trị dòng 3 (ví dụ: -6.000.000)

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public List<DebtEntry>? Entries { get; set; }
    }
}
