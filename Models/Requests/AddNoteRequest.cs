namespace Telemedicine.API.Models.Requests
{
    public class AddNoteRequest
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
