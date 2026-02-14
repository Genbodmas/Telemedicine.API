namespace Telemedicine.API.Services
{
    public interface IUserContextService
    {
        int GetUserId();
        string GetUserRole();
    }
}
