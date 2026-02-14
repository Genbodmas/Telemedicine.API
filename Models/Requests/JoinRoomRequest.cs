namespace Telemedicine.API.Models.Requests
{
    public class JoinRoomRequest
    {
        public Guid RoomId { get; set; }
        public int UserId { get; set; }
    }
}
