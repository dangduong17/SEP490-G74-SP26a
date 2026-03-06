using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
        Task<AdminUserListViewModel> GetUserListAsync(string? keyword, string? role, string? status, int page, int pageSize);
        Task<AdminUpdateUserViewModel?> GetUpdateUserAsync(int id);
        Task<ServiceResult> CreateAdminAsync(AdminCreateAdminViewModel model);
        Task<ServiceResult> CreateCandidateAsync(AdminCreateCandidateViewModel model);
        Task<ServiceResult> CreateRecruiterAsync(AdminCreateRecruiterViewModel model);
        Task<ServiceResult> UpdateUserAsync(AdminUpdateUserViewModel model);
        Task<ServiceResult> SoftDeleteUserAsync(int id);
    }
}
