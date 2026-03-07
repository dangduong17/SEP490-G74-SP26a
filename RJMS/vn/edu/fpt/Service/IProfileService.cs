using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IProfileService
    {
        // Candidate
        Task<UserProfileDTO?> GetPersonalProfileAsync(string userId);
<<<<<<< Updated upstream
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
=======

        // Recruiter
        Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId);
        Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model);
>>>>>>> Stashed changes
    }
}
