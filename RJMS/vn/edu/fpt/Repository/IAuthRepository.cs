using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IAuthRepository
    {
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<object?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<bool> UpdatePasswordHashAsync(string email, string newPasswordHash);
        Task<User?> CreateUserAsync(User user);
        Task<bool> SetEmailConfirmedAsync(string email);
        Task<Candidate?> CreateCandidateAsync(Candidate candidate);
        Task<string> GetUserRoleAsync(int userId);
    }
}
