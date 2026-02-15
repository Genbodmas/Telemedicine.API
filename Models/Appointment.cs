using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicine.API.Models
{
    [Table("tbl_Appointments")]
    public class Appointment
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public User Doctor { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public User Patient { get; set; }

        public DateTime ScheduledTime { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Active, Completed

        public string? Reason { get; set; }
    }
}
