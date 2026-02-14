namespace Telemedicine.API.Models.Requests
{
    public class EndSessionRequest
    {
        public Guid RoomId { get; set; }
        public int ActionBy { get; set; }
    }
}
