using System.ComponentModel.DataAnnotations;

namespace Telemedicine.API.Models.Requests
{
    public class AddRecommendationRequest
    {
        [Required]
        public Guid RoomId { get; set; }
        [Required]
        public string Details { get; set; }
    }
}
