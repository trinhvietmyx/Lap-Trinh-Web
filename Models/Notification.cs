using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? OrderId { get; set; }

        [Required]
        [StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(600)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string? OrderStatus { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }
    }
}
