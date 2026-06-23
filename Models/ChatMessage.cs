using System;
using System.ComponentModel.DataAnnotations;

namespace GIUA_KY_WEB_3AE.Models
{
    /// <summary>
    /// Lớp định nghĩa đối tượng Tin nhắn chat (ChatMessage).
    /// </summary>
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Sender { get; set; } = "User"; // "User" hoặc "AI"

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
