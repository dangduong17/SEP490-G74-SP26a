using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IProfileService
    {
        // ── Candidate ─────────────────────────────────────────────────────────
        Task<UserProfileDTO?> GetPersonalProfileAsync(string userId);
        Task<CandidateEditProfileViewModel?> GetCandidateProfileForEditAsync(int userId);
        Task<bool> UpdateCandidateProfileAsync(int userId, CandidateEditProfileViewModel model);

        // ── Password ──────────────────────────────────────────────────────────
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        // ── Recruiter ─────────────────────────────────────────────────────────
        Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId);
        Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model);

        Task<RecruiterEditProfileViewModel?> GetRecruiterProfileForEditAsync(int userId);
        Task<bool> UpdateRecruiterProfileNewAsync(int userId, RecruiterEditProfileViewModel model);

        Task<CompanyEditProfileViewModel?> GetCompanyProfileForEditAsync(int userId);
        Task<bool> UpdateCompanyProfileAsync(int userId, CompanyEditProfileViewModel model);

        // ── Admin ─────────────────────────────────────────────────────────────
        Task<AdminEditProfileViewModel?> GetAdminProfileForEditAsync(int userId);
        Task<bool> UpdateAdminProfileAsync(int userId, AdminEditProfileViewModel model);
    }
}
