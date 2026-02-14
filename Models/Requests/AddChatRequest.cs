namespace Telemedicine.API.Models.Requests
{
    public class AddChatRequest
    {
        public Guid RoomId { get; set; }
        public int SenderId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
    }
}
