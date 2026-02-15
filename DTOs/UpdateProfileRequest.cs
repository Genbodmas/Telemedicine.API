using Telemedicine.API.Models;

namespace Telemedicine.API.DTOs
{
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Specialty { get; set; }
    }
}
