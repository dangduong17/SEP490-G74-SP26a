using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IRecruiterManagementRepository
    {
        // ── Company resolution ─────────────────────────────────────────────
        Task<Recruiter?> GetRecruiterByUserIdAsync(int userId);
        Task<Company?>   GetCompanyByIdAsync(int companyId);

        // ── Company Locations ──────────────────────────────────────────────
        Task<List<CompanyLocation>> GetCompanyLocationsAsync(int companyId);
        Task<CompanyLocation?>      GetCompanyLocationByIdAsync(int id);
        Task AddCompanyLocationAsync(CompanyLocation location);
        Task<Location?>  GetMatchingLocationAsync(int? provinceCode, int? wardCode, string? address);
        Task AddLocationAsync(Location location);
        Task SaveChangesAsync();

        // ── Employees ─────────────────────────────────────────────────────
        Task<(int total, List<Recruiter> items)> GetEmployeesPagedAsync(
            int companyId, string? keyword, int page, int pageSize);
        Task<Recruiter?> GetEmployeeByIdAsync(int recruiterId, int companyId);
        Task<User?>      GetUserByIdAsync(int userId);
        Task<bool>       UserEmailExistsAsync(string email);

        Task<User>     CreateUserAsync(User user);
        Task           AssignRoleAsync(int userId, string roleName);
        Task<Role?>    GetRoleByNameAsync(string roleName);
        Task<UserRole> AddUserRoleAsync(UserRole userRole);

        Task<Recruiter> AddRecruiterAsync(Recruiter recruiter);
        Task            UpdateRecruiterAsync(Recruiter recruiter);
        Task            UpdateUserAsync(User user);

        Task<List<RecruiterLocation>> GetRecruiterLocationsAsync(int recruiterId);
        Task AddRecruiterLocationAsync(RecruiterLocation rl);
        Task RemoveRecruiterLocationAsync(RecruiterLocation rl);
        Task RemoveAllRecruiterLocationsAsync(int recruiterId);

        Task SetUserActiveAsync(int userId, bool isActive);
    }
}
