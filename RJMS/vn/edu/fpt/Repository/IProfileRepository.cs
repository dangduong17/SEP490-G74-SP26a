using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IProfileRepository
    {
        Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash);
    }
}
