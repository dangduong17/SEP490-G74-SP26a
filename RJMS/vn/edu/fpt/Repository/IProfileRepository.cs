using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IProfileRepository
    {
        // ── Candidate ─────────────────────────────────────────────────────────
        Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId);
        Task<CandidateEditProfileViewModel?> GetCandidateProfileForEditAsync(int userId);
        Task<bool> UpdateCandidateProfileAsync(int userId, CandidateEditProfileViewModel model);

        // ── Password ──────────────────────────────────────────────────────────
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash);

        // ── Recruiter ─────────────────────────────────────────────────────────
        Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId);
        Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model);

        Task<RecruiterEditProfileViewModel?> GetRecruiterProfileForEditAsync(int userId);
        Task<bool> UpdateRecruiterProfileNewAsync(int userId, RecruiterEditProfileViewModel model);

        // ── Admin ─────────────────────────────────────────────────────────────
        Task<AdminEditProfileViewModel?> GetAdminProfileForEditAsync(int userId);
        Task<bool> UpdateAdminProfileAsync(int userId, AdminEditProfileViewModel model);
    }
}
