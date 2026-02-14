using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;
using Telemedicine.API.DTOs;

namespace Telemedicine.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IConsultationRepository _repository;
        private readonly IUserContextService _userContext;

        public AvailabilityController(IConsultationRepository repository, IUserContextService userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        /// <summary>Set availability for a day of the week (Doctor only)</summary>
        [HttpPost("set")]
        public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityDto dto)
        {
            var doctorId = _userContext.GetUserId();
            // Pass doctorId and DTO to repo
            var result = await _repository.SetDoctorAvailabilityAsync(doctorId, dto);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>Get availability for a doctor</summary>
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetAvailability(int doctorId)
        {
            var result = await _repository.GetDoctorAvailabilityAsync(doctorId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        /// <summary>Get my own availability (Doctor)</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAvailability()
        {
            var doctorId = _userContext.GetUserId();
            var result = await _repository.GetDoctorAvailabilityAsync(doctorId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        /// <summary>Delete an availability slot</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var doctorId = _userContext.GetUserId();
            var result = await _repository.DeleteDoctorAvailabilityAsync(id, doctorId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }
    }


}
