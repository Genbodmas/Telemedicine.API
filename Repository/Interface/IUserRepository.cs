using Telemedicine.API.Models;
using Telemedicine.API.DTOs;

namespace Telemedicine.API.Repository.Interface
{
    public interface IUserRepository
    {
        Task<ApiResponse<User>> GetProfileAsync(int userId);
        Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<ApiResponse<string>> UploadAvatarAsync(int userId, IFormFile file);
    }
}
