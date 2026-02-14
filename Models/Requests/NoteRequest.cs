namespace Telemedicine.API.Models.Requests
{
    public class NoteRequest
    {
        public int AppointmentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
