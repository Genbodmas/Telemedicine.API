using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemedicine.API.DTOs;
using Telemedicine.API.Models;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;
using System.Linq;

namespace Telemedicine.API.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IConsultationRepository _repository;
        private readonly IUserContextService _userContextService;

        public AnalyticsController(IConsultationRepository repository, IUserContextService userContextService)
        {
            _repository = repository;
            _userContextService = userContextService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var doctorId = _userContextService.GetUserId();
            

            
            var appointmentsResult = await _repository.GetDoctorAppointmentsAsync(doctorId);
            if (!appointmentsResult.Succeeded) return StatusCode(500, "Failed to fetch data");

            var appointments = appointmentsResult.Data;
            
            var summary = new DashboardSummaryDto
            {
                TotalAppointments = appointments.Count,
                TotalPatients = appointments.Select(a => a.PatientId).Distinct().Count(),
                UpcomingAppointments = appointments.Count(a => a.ScheduledTime > DateTime.UtcNow && a.Status == "Pending")
            };


            var today = DateTime.UtcNow.Date;
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = appointments.Count(a => a.ScheduledTime.Date == date);
                summary.WeeklyTrend.Add(new DailyStatDto 
                { 
                    Date = date.ToString("ddd"), // Mon, Tue...
                    Count = count 
                });
            }

            return Ok(new ApiResponse<DashboardSummaryDto> { Succeeded = true, Data = summary });
        }
    }
}
