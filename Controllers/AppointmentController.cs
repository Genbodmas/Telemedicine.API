using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Telemedicine.API.Hubs;
using Telemedicine.API.Models;
using Telemedicine.API.Models.Requests;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;

namespace Telemedicine.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IConsultationRepository _repository;
        private readonly IUserContextService _userContext;

        private readonly IHubContext<TelemedicineHub> _hubContext;

        public AppointmentController(IConsultationRepository repository, IUserContextService userContext, IHubContext<TelemedicineHub> hubContext)
        {
            _repository = repository;
            _userContext = userContext;
            _hubContext = hubContext;
        }

        [HttpGet("doctors")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<User>>), 200)]
        public async Task<IActionResult> GetDoctors()
        {
            var result = await _repository.GetDoctorsAsync();
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPost("book")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto request)
        {
            var patientId = _userContext.GetUserId();
            var result = await _repository.BookAppointmentAsync(patientId, request.DoctorId, request.ScheduledTime, request.Reason);
            
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);

            // Notify Doctor via SignalR
            await _hubContext.Clients.User(request.DoctorId.ToString()).SendAsync("ReceiveNotification", "New appointment booked!");
            
            return Ok(result);
        }

        [HttpGet("my-appointments")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<dynamic>>), 200)]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = _userContext.GetUserId();
            var role = _userContext.GetUserRole();

            var result = await _repository.GetUserAppointmentsAsync(userId, role);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }
    }

    public class BookAppointmentDto
    {
        public int DoctorId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Reason { get; set; }
    }
}
