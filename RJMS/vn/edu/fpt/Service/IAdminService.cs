using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IAdminService
    {
        // Users management
        Task<AdminDashboardViewModel> GetDashboardAsync();
        Task<AdminUserListViewModel> GetUserListAsync(string? keyword, string? role, string? status, int page, int pageSize);
        Task<AdminUpdateUserViewModel?> GetUpdateUserAsync(int id);
        Task<ServiceResult> CreateAdminAsync(AdminCreateAdminViewModel model);
        Task<ServiceResult> CreateCandidateAsync(AdminCreateCandidateViewModel model);
        Task<ServiceResult> CreateRecruiterAsync(AdminCreateRecruiterViewModel model);
        Task<ServiceResult> UpdateUserAsync(AdminUpdateUserViewModel model);
        Task<ServiceResult> SoftDeleteUserAsync(int id);

        // Skills management
        Task<AdminSkillListViewModel> GetSkillListAsync(string? keyword, string? category, int page, int pageSize);
        Task<AdminUpdateSkillViewModel?> GetSkillForEditAsync(int id);
        Task<ServiceResult> CreateSkillAsync(AdminCreateSkillViewModel model);
        Task<ServiceResult> UpdateSkillAsync(AdminUpdateSkillViewModel model);
        Task<ServiceResult> DeleteSkillAsync(int id);

        // Companies management
        Task<AdminCompanyListViewModel> GetCompanyListAsync(string? keyword, string? industry, string? verificationStatus, int page, int pageSize);
        Task<AdminCompanyDetailViewModel?> GetCompanyDetailAsync(int id);
        Task<ServiceResult> VerifyCompanyAsync(int id);
        Task<ServiceResult> UnverifyCompanyAsync(int id);

        // Subscriptions management
        Task<AdminSubscriptionListViewModel> GetSubscriptionListAsync(string? keyword, string? status, int? planId, int page, int pageSize);
        Task<AdminSubscriptionDetailViewModel?> GetSubscriptionDetailAsync(int id);
    }
}
