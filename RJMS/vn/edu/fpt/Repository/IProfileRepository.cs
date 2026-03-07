using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IProfileRepository
    {
        // ── Candidate ─────────────────────────────────────────────────────────
        Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId);

        // ── Password ──────────────────────────────────────────────────────────
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash);

        // ── Recruiter ─────────────────────────────────────────────────────────
        Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId);
        Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model);
    }
}
