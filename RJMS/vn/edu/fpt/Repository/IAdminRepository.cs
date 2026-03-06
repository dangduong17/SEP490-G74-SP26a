using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IAdminRepository
    {
        // User
        Task<bool> UserEmailExistsAsync(string email, int? excludeId = null);
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByIdWithDetailsAsync(int id);
        Task<(int total, List<User> items)> GetUsersPagedAsync(
            string? keyword, string? role, string? status, int page, int pageSize);
        Task<(int total, int active, int inactive, int admins, int candidates, int recruiters)> GetDashboardStatsAsync();
        Task UpdateUserAsync(User user);
        Task SoftDeleteUserAsync(User user);

        // Roles / UserRoles
        Task<Role?> GetRoleByNameAsync(string name);
        Task<UserRole?> GetUserRoleAsync(int userId);
        Task AddUserRoleAsync(UserRole userRole);
        Task RemoveUserRoleAsync(UserRole userRole);

        // Candidate
        Task AddCandidateAsync(Candidate candidate);
        Task UpdateCandidateAsync(Candidate candidate);

        // Recruiter
        Task AddRecruiterAsync(Recruiter recruiter);
        Task UpdateRecruiterAsync(Recruiter recruiter);

        // Company
        Task<bool> CompanyTaxCodeExistsAsync(string taxCode);
        Task AddCompanyAsync(Company company);
        Task<int> SaveChangesAsync();
    }
}
