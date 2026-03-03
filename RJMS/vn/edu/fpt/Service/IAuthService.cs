using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> LoginAsync(LoginDTO loginDto);
        Task<bool> LogoutAsync();
    }
}
