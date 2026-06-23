using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Báo cáo tháng (MonthlySnapshot).
    /// </summary>
    public class MonthlySnapshot
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Năm")]
        public int Year { get; set; }

        [Display(Name = "Tháng")]
        public int Month { get; set; }

        [Display(Name = "Số lượng đơn")]
        public int OrderCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng thu")]
        public decimal TotalIncome { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng chi")]
        public decimal TotalExpense { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Lợi nhuận")]
        public decimal NetProfit { get; set; }
    }
}
