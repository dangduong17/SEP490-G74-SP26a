using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<(bool Success, string Message)> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                var userExists = await _authRepository.UserExistsAsync(loginDto.Email);
                if (!userExists)
                {
                    return (false, "User not found");
                }

                var isValid = await _authRepository.ValidateUserCredentialsAsync(
                    loginDto.Email, 
                    loginDto.Password
                );

                if (!isValid)
                {
                    return (false, "Invalid credentials");
                }

                return (true, "Login successful");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<bool> LogoutAsync()
        {
            await Task.CompletedTask;
            return true;
        }
    }
}
