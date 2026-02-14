using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicine.API.Models
{
    [Table("tbl_Audits")]
    public class Audit
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        public int? ActionBy { get; set; }
        [ForeignKey("ActionBy")]
        public User? User { get; set; }

        [Required]
        public string ActionStatus { get; set; } = string.Empty;

        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        public string? Details { get; set; }
    }
}
