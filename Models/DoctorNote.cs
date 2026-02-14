using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicine.API.Models
{
    [Table("tbl_DoctorNotes")]
    public class DoctorNote
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty; // Encrypted

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
