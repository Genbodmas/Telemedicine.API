using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicine.API.Models
{
    [Table("tbl_Chats")]
    public class Chat
    {
        public int Id { get; set; }

        public Guid RoomId { get; set; }
        [ForeignKey("RoomId")]
        public ConsultationRoom Room { get; set; }

        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty; // Encrypted

        public string? FileUrl { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
