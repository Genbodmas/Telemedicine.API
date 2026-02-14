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
    }
}
