using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Telemedicine.API.Models.Requests;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;

namespace Telemedicine.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationRepository _repository;
        private readonly EncryptionService _encryptionService;
        private readonly FileUploadService _fileUploadService;
        private readonly IUserContextService _userContextService;
        private readonly PdfService _pdfService;

        public ConsultationController(IConsultationRepository repository, EncryptionService encryptionService, FileUploadService fileUploadService, IUserContextService userContextService, PdfService pdfService)
        {
            _repository = repository;
            _encryptionService = encryptionService;
            _fileUploadService = fileUploadService;
            _userContextService = userContextService;
            _pdfService = pdfService;
        }

        [HttpPost("generate-pdf/{roomId}")]
        public async Task<IActionResult> GeneratePdf(Guid roomId)
        {

            var roomRes = await _repository.GetRoomDetailsAsync(roomId);
            if (!roomRes.Succeeded || roomRes.Data == null) return NotFound("Room not found");
            
            var details = roomRes.Data;


            var recRes = await _repository.GetRecommendationAsync(roomId);
            string recommendation = "No recommendation.";
            if (recRes.Succeeded && recRes.Data != null)
            {
                 try { recommendation = _encryptionService.Decrypt(recRes.Data); } catch { recommendation = recRes.Data; }
            }


            var pdfBytes = _pdfService.GenerateConsultationSummary(details.DoctorName, details.PatientName, details.ScheduledTime, recommendation);


            string fileName = $"Summary_{details.PatientName}_{details.ScheduledTime:yyyyMMdd}.pdf";
            var url = await _fileUploadService.UploadFileAsync(pdfBytes, fileName);

            return Ok(new { Url = url });
        }

        [HttpPost("start/{appointmentId}")]
        public async Task<IActionResult> StartConsultation(int appointmentId)
        {
            var doctorId = _userContextService.GetUserId();
            var request = new CreateRoomRequest { AppointmentId = appointmentId, ActionBy = doctorId };

            var response = await _repository.CreateRoomAsync(request);

            if (response.Succeeded && response.Data != Guid.Empty)
            {
                var roomUrl = $"{Request.Scheme}://{Request.Host}/room/{response.Data}";
                return Ok(new { RoomId = response.Data, Url = roomUrl });
            }

            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpPost("notes-room")]
        public async Task<IActionResult> AddNoteByRoom([FromBody] AddNoteByRoomRequest request)
        {
            var doctorId = _userContextService.GetUserId();
            

            var roomDetails = await _repository.GetRoomDetailsAsync(request.RoomId);
            if (!roomDetails.Succeeded || roomDetails.Data == null)
            {
                return StatusCode(404, "Room not found");
            }

            var appointmentId = roomDetails.Data.AppointmentId;
            var encryptedContent = _encryptionService.Encrypt(request.Content);

            var addNoteRequest = new AddNoteRequest
            {
                AppointmentId = appointmentId,
                DoctorId = doctorId,
                Content = encryptedContent
            };

            var response = await _repository.AddNoteAsync(addNoteRequest);

            if (response.Succeeded)
            {
                return Ok(response.Message);
            }

            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpPost("notes")]
        public async Task<IActionResult> AddNote([FromBody] NoteRequest requestDto)
        {
            var doctorId = _userContextService.GetUserId();
            var encryptedContent = _encryptionService.Encrypt(requestDto.Content);

            var request = new AddNoteRequest
            {
                AppointmentId = requestDto.AppointmentId,
                DoctorId = doctorId,
                Content = encryptedContent
            };

            var response = await _repository.AddNoteAsync(request);

            if (response.Succeeded)
            {
                return Ok(response.Message);
            }

            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                var url = await _fileUploadService.UploadFileAsync(file);
                return Ok(new { Url = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
        }

        [HttpGet("notes/{roomId}")]
        public async Task<IActionResult> GetNotes(Guid roomId)
        {
            var result = await _repository.GetConsultationNotesAsync(roomId);
            if (result.Succeeded)
            {
                // Decrypt notes
                foreach (var note in result.Data)
                {
                    try 
                    { 
                        note.Content = _encryptionService.Decrypt(note.Content); 
                    } 
                    catch 
                    {
                        // Keep original if decryption fails
                    }
                }
                return Ok(result);
            }
            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpPost("recommendation")]
        public async Task<IActionResult> AddRecommendation([FromBody] AddRecommendationRequest request)
        {
            var doctorId = _userContextService.GetUserId();
            var encryptedDetails = _encryptionService.Encrypt(request.Details);

            var result = await _repository.AddRecommendationAsync(doctorId, request.RoomId, encryptedDetails);
            if (result.Succeeded) return Ok(result);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("recommendation/{roomId}")]
        public async Task<IActionResult> GetRecommendation(Guid roomId)
        {
            var result = await _repository.GetRecommendationAsync(roomId);
            if (result.Succeeded && result.Data != null)
            {
                try 
                { 
                    result.Data = _encryptionService.Decrypt(result.Data); 
                } 
                catch {}
            }
            return Ok(result);
        }

        [HttpGet("history/{roomId}")]
        public async Task<IActionResult> GetChatHistory(Guid roomId)
        {
            var result = await _repository.GetChatHistoryAsync(roomId);
            if (result.Succeeded) return Ok(result);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("room-details/{roomId}")]
        public async Task<IActionResult> GetRoomDetails(Guid roomId)
        {
            var result = await _repository.GetRoomDetailsAsync(roomId);
            if (result.Succeeded) return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
    }
}
