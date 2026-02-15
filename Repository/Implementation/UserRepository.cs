using Microsoft.EntityFrameworkCore;
using Telemedicine.API.Data;
using Telemedicine.API.DTOs;
using Telemedicine.API.Models;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.Services;

namespace Telemedicine.API.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public UserRepository(AppDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<ApiResponse<User>> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ApiResponse<User>.Fail("User not found", 404);
            
            // Should probably map to DTO to hide password hash, but for now returning User as requested in plan
            user.PasswordHash = ""; // Security check
            return ApiResponse<User>.Success("Profile found", user);
        }

        public async Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ApiResponse<bool>.Fail("User not found", 404);

            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            if (request.Bio != null) user.Bio = request.Bio;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (request.Address != null) user.Address = request.Address;
            if (request.Specialty != null) user.Specialty = request.Specialty;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Success("Profile updated", true);
        }

        public async Task<ApiResponse<string>> UploadAvatarAsync(int userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ApiResponse<string>.Fail("User not found", 404);

            try
            {
                var url = await _fileUploadService.UploadFileAsync(file);
                user.ProfilePictureUrl = url;
                await _context.SaveChangesAsync();
                return ApiResponse<string>.Success("Avatar uploaded", url);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(ex.Message, 500);
            }
        }
    }
}
