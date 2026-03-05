using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> LoginAsync(LoginDTO loginDto);
        Task<bool> LogoutAsync();
        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDTO forgotDto);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDTO registerDto);
        Task<(bool Success, string Message)> ConfirmEmailAsync(string token);
    }
}
