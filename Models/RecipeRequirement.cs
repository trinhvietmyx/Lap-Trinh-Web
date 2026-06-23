using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Định lượng công thức (RecipeRequirement).
    /// </summary>
    public class RecipeRequirement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IngredientId { get; set; }

        [ForeignKey("IngredientId")]
        public Ingredient? Ingredient { get; set; }

        [Required]
        public int SellableItemId { get; set; }

        [ForeignKey("SellableItemId")]
        public SellableItem? SellableItem { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Số lượng sử dụng")]
        public decimal QuantityUsed { get; set; }
    }
}
