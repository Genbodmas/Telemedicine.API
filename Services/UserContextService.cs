using System.Security.Claims;

namespace Telemedicine.API.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("No user context.");
            }

            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            // Fallback for different JWT mappers
            if (userIdClaim == null)
            {
                userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "id");
            }

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            
            throw new UnauthorizedAccessException("Invalid User");
        }

        public string GetUserRole()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return string.Empty;
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public string GetUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return "Unknown";
            
            // Try specific claim types or standard Name claim
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value 
                ?? user.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name")?.Value 
                ?? "User " + GetUserId();
        }
    }
}
