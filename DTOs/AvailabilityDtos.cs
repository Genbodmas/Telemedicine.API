namespace Telemedicine.API.DTOs
{
    public class SetAvailabilityDto
    {
        public int DayOfWeek { get; set; }     // 0=Sun, 1=Mon, ..., 6=Sat
        public string StartTime { get; set; } = ""; // "09:00"
        public string EndTime { get; set; } = "";   // "17:00"
        public bool IsActive { get; set; } = true;
    }

    public class AvailabilitySlotDto
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
