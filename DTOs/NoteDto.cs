namespace Telemedicine.API.DTOs
{
    public class NoteDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DoctorName { get; set; }
    }
}
