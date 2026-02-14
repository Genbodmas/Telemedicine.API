namespace Telemedicine.API.Models.Requests
{
    public class CreateRoomRequest
    {
        public int AppointmentId { get; set; }
        public int ActionBy { get; set; } // UserId
    }
}
