using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Kỳ nợ/Kỳ thanh toán (DebtEntry).
    /// </summary>
    public class DebtEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DebtLedgerId { get; set; }

        [ForeignKey("DebtLedgerId")]
        public DebtLedger? DebtLedger { get; set; }

        [Display(Name = "Kỳ số")]
        public int PeriodNumber { get; set; } // 1, 2, 3...

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; } // ví dụ: -800.000, 13.800.000

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Chờ"; // "Chờ", "Đã thanh toán"
    }
}
