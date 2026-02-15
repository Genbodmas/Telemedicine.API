using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicine.API.Models
{
    [Table("tbl_Users")]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Patient"; // "Doctor" or "Patient"

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Specialty { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
