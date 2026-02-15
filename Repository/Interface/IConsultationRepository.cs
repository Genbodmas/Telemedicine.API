using Telemedicine.API.Models;
using Telemedicine.API.Models.Requests;
using Telemedicine.API.DTOs;

namespace Telemedicine.API.Repository.Interface
{
    public interface IConsultationRepository
    {
        Task<ApiResponse<Guid>> CreateRoomAsync(CreateRoomRequest request);
        Task<ApiResponse<bool>> JoinRoomAsync(JoinRoomRequest request);
        Task<ApiResponse<bool>> AddChatAsync(AddChatRequest request);
        Task<ApiResponse<bool>> AddNoteAsync(AddNoteRequest request);
        Task<ApiResponse<bool>> EndSessionAsync(EndSessionRequest request);


        Task<ApiResponse<IEnumerable<User>>> GetDoctorsAsync();
        Task<ApiResponse<int>> BookAppointmentAsync(int patientId, int doctorId, DateTime scheduledTime, string reason);
        Task<ApiResponse<RoomDetailsDto>> GetRoomDetailsAsync(Guid roomId);
        Task<ApiResponse<IEnumerable<dynamic>>> GetUserAppointmentsAsync(int userId, string role);
        Task<ApiResponse<IEnumerable<dynamic>>> GetChatHistoryAsync(Guid roomId);


        Task<ApiResponse<bool>> SetDoctorAvailabilityAsync(int doctorId, SetAvailabilityDto request);
        Task<ApiResponse<List<AvailabilitySlotDto>>> GetDoctorAvailabilityAsync(int doctorId);
        Task<ApiResponse<bool>> DeleteDoctorAvailabilityAsync(int id, int doctorId);


        Task<ApiResponse<List<NoteDto>>> GetConsultationNotesAsync(Guid roomId);
        Task<ApiResponse<bool>> AddRecommendationAsync(int doctorId, Guid roomId, string details);
        Task<ApiResponse<string>> GetRecommendationAsync(Guid roomId);


        Task<ApiResponse<List<Appointment>>> GetDoctorAppointmentsAsync(int doctorId);
    }
}
