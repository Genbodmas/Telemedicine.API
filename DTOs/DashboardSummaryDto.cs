namespace Telemedicine.API.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public decimal Revenue { get; set; }
        public List<DailyStatDto> WeeklyTrend { get; set; } = new List<DailyStatDto>();
    }

    public class DailyStatDto
    {
        public string Date { get; set; } // "Mon", "Tue" etc or "YYYY-MM-DD"
        public int Count { get; set; }
    }
}
