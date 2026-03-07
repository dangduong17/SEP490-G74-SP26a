using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IProfileService
    {
        Task<UserProfileDTO?> GetPersonalProfileAsync(string userId);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
