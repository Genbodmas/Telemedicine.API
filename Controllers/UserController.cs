using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemedicine.API.DTOs;
using Telemedicine.API.Models;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;

namespace Telemedicine.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserContextService _userContextService;

        public UserController(IUserRepository userRepository, IUserContextService userContextService)
        {
            _userRepository = userRepository;
            _userContextService = userContextService;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = _userContextService.GetUserId();
            var result = await _userRepository.GetProfileAsync(userId);
            if (!result.Succeeded) return StatusCode(result.StatusCode, result.Message);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = _userContextService.GetUserId();
            var result = await _userRepository.UpdateProfileAsync(userId, request);
            if (!result.Succeeded) return StatusCode(result.StatusCode, result.Message);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userId = _userContextService.GetUserId();
            var result = await _userRepository.UploadAvatarAsync(userId, file);
            if (!result.Succeeded) return StatusCode(result.StatusCode, result.Message);
            return Ok(result);
        }
    }
}
