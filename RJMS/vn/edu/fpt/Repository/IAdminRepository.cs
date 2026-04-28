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

        // Location
        Task<Location?> GetLocationAsync(int? provinceCode, int? wardCode, string? address);
        Task AddLocationAsync(Location location);
        Task AddCompanyLocationAsync(CompanyLocation companyLocation);
        Task AddRecruiterLocationAsync(RecruiterLocation recruiterLocation);
        Task<List<RecruiterLocation>> GetRecruiterLocationsByRecruiterIdAsync(int recruiterId);
        Task RemoveRecruiterLocationAsync(RecruiterLocation recruiterLocation);
        Task<List<CompanyLocation>> GetCompanyLocationsAsync(int companyId);
        Task<CompanyLocation?> GetCompanyLocationByIdAsync(int id);
        Task DeleteCompanyLocationAsync(CompanyLocation companyLocation);

        // Employee
        Task<(int total, List<Recruiter> items)> GetEmployeesPagedAsync(string? keyword, int? companyId, int page, int pageSize);
        Task<List<RecruiterLocation>> GetRecruiterLocationsAsync(int recruiterId);

        // Skills
        Task<(int total, List<Skill> items)> GetSkillsPagedAsync(string? keyword, string? category, int page, int pageSize);
        Task<List<string>> GetSkillCategoriesAsync();
        Task<Skill?> GetSkillByIdAsync(int id);
        Task<bool> SkillNameExistsAsync(string name, int? excludeId = null);
        Task AddSkillAsync(Skill skill);
        Task UpdateSkillAsync(Skill skill);
        Task DeleteSkillAsync(Skill skill);

        // Companies
        Task<List<Company>> GetAllCompaniesAsync();
        Task<(int total, List<Company> items)> GetCompaniesPagedAsync(string? keyword, string? industry, string? verificationStatus, int page, int pageSize);
        Task<Company?> GetCompanyByIdWithDetailsAsync(int id);
        Task UpdateCompanyAsync(Company company);

        // Subscriptions
        Task<(int total, List<Subscription> items)> GetSubscriptionsPagedAsync(string? keyword, string? status, int? planId, int page, int pageSize);
        Task<Subscription?> GetSubscriptionByIdWithDetailsAsync(int id);
        Task<Subscription?> GetSubscriptionByIdAsync(int id);
        Task UpdateSubscriptionAsync(Subscription subscription);
        Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync();
        Task<List<string>> GetSubscriptionStatusesAsync();

        // Jobs
        IQueryable<Job> GetJobsQuery();
        Task<Job?> GetJobByIdAsync(int id);
        Task<Job?> GetJobByIdWithDetailsAsync(int id);
        Task UpdateJobAsync(Job job);
    }
}
