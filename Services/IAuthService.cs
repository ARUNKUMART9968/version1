namespace BoticAPI.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message, string? token)> LoginAsync(string email, string password);
        Task<(bool success, string message, int? userId)> RegisterAsync(string name, string email, string password, int roleId);
    }
}