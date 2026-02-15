using System.ComponentModel.DataAnnotations;

namespace Telemedicine.API.Models.Requests
{
    public class AddNoteByRoomRequest
    {
        [Required]
        public Guid RoomId { get; set; }
        
        [Required]
        public string Content { get; set; }
    }
}
