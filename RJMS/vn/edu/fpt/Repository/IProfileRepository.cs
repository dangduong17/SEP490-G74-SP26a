using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IProfileRepository
    {
        // Candidate
        Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId);
<<<<<<< Updated upstream
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash);
=======

        // Recruiter
        Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId);
        Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model);
>>>>>>> Stashed changes
    }
}
