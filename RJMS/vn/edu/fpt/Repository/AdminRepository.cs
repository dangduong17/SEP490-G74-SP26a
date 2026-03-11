using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly FindingJobsDbContext _db;

        public AdminRepository(FindingJobsDbContext db)
        {
            _db = db;
        }

        // ── User ─────────────────────────────────────────────────────────────

        public Task<bool> UserEmailExistsAsync(string email, int? excludeId = null)
        {
            return excludeId.HasValue
                ? _db.Users.AnyAsync(u => u.Email == email && u.Id != excludeId.Value)
                : _db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public Task<User?> GetUserByIdWithDetailsAsync(int id)
        {
            return _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Candidates)
                .Include(u => u.Recruiters)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<(int total, List<User> items)> GetUsersPagedAsync(
            string? keyword, string? role, string? status, int page, int pageSize)
        {
            var query = _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Candidates)
                .Include(u => u.Recruiters)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(key)) ||
                    ((u.FirstName + " " + u.LastName).ToLower().Contains(key)) ||
                    (u.Phone != null && u.Phone.Contains(key)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bool isActive = status.Equals("active", StringComparison.OrdinalIgnoreCase);
                query = isActive
                    ? query.Where(u => u.IsActive == true)
                    : query.Where(u => u.IsActive != true);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public async Task<(int total, int active, int inactive, int admins, int candidates, int recruiters)> GetDashboardStatsAsync()
        {
            var users = await _db.Users.AsNoTracking().ToListAsync();
            var userRoles = await _db.UserRoles.Include(ur => ur.Role).AsNoTracking().ToListAsync();
            return (
                users.Count,
                users.Count(u => u.IsActive == true),
                users.Count(u => u.IsActive != true),
                userRoles.Count(ur => ur.Role.Name == "Admin"),
                userRoles.Count(ur => ur.Role.Name == "Candidate"),
                userRoles.Count(ur => ur.Role.Name == "Recruiter")
            );
        }

        public async Task UpdateUserAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteUserAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        // ── Roles / UserRoles ─────────────────────────────────────────────────

        public Task<Role?> GetRoleByNameAsync(string name)
        {
            return _db.Roles.FirstOrDefaultAsync(r => r.Name == name);
        }

        public Task<UserRole?> GetUserRoleAsync(int userId)
        {
            return _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
        }

        public async Task AddUserRoleAsync(UserRole userRole)
        {
            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveUserRoleAsync(UserRole userRole)
        {
            _db.UserRoles.Remove(userRole);
            await _db.SaveChangesAsync();
        }

        // ── Candidate ─────────────────────────────────────────────────────────

        public async Task AddCandidateAsync(Candidate candidate)
        {
            _db.Candidates.Add(candidate);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateCandidateAsync(Candidate candidate)
        {
            _db.Candidates.Update(candidate);
            await _db.SaveChangesAsync();
        }

        // ── Recruiter ─────────────────────────────────────────────────────────

        public async Task AddRecruiterAsync(Recruiter recruiter)
        {
            _db.Recruiters.Add(recruiter);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRecruiterAsync(Recruiter recruiter)
        {
            _db.Recruiters.Update(recruiter);
            await _db.SaveChangesAsync();
        }

        // ── Company ───────────────────────────────────────────────────────────

        public Task<bool> CompanyTaxCodeExistsAsync(string taxCode)
        {
            return _db.Companies.AnyAsync(c => c.TaxCode == taxCode);
        }

        public async Task AddCompanyAsync(Company company)
        {
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }

        // ── Skills ────────────────────────────────────────────────────────────

        public async Task<(int total, List<Skill> items)> GetSkillsPagedAsync(string? keyword, string? category, int page, int pageSize)
        {
            var query = _db.Skills
                .Include(s => s.JobSkills)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(key));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category == category);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public async Task<List<string>> GetSkillCategoriesAsync()
        {
            return await _db.Skills
                .Where(s => s.Category != null)
                .Select(s => s.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public Task<Skill?> GetSkillByIdAsync(int id)
        {
            return _db.Skills.FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<bool> SkillNameExistsAsync(string name, int? excludeId = null)
        {
            return excludeId.HasValue
                ? _db.Skills.AnyAsync(s => s.Name == name && s.Id != excludeId.Value)
                : _db.Skills.AnyAsync(s => s.Name == name);
        }

        public async Task AddSkillAsync(Skill skill)
        {
            _db.Skills.Add(skill);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateSkillAsync(Skill skill)
        {
            _db.Skills.Update(skill);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteSkillAsync(Skill skill)
        {
            _db.Skills.Remove(skill);
            await _db.SaveChangesAsync();
        }

        // ── Companies ─────────────────────────────────────────────────────────

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _db.Companies
                .Where(c => c.IsVerified == true)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<(int total, List<Company> items)> GetCompaniesPagedAsync(string? keyword, string? industry, string? verificationStatus, int page, int pageSize)
        {
            var query = _db.Companies
                .Include(c => c.Recruiters)
                .Include(c => c.Jobs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLower();
                query = query.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(key)) ||
                    (c.TaxCode != null && c.TaxCode.ToLower().Contains(key)) ||
                    (c.Email != null && c.Email.ToLower().Contains(key)));
            }

            if (!string.IsNullOrWhiteSpace(industry))
            {
                query = query.Where(c => c.Industry == industry);
            }

            if (!string.IsNullOrWhiteSpace(verificationStatus))
            {
                bool isVerified = verificationStatus.Equals("verified", StringComparison.OrdinalIgnoreCase);
                query = isVerified
                    ? query.Where(c => c.IsVerified == true)
                    : query.Where(c => c.IsVerified != true);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public Task<Company?> GetCompanyByIdWithDetailsAsync(int id)
        {
            return _db.Companies
                .Include(c => c.Recruiters).ThenInclude(r => r.User)
                .Include(c => c.Jobs)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            _db.Companies.Update(company);
            await _db.SaveChangesAsync();
        }

        // ── Subscriptions ─────────────────────────────────────────────────────

        public async Task<(int total, List<Subscription> items)> GetSubscriptionsPagedAsync(string? keyword, string? status, int? planId, int page, int pageSize)
        {
            var query = _db.Subscriptions
                .Include(s => s.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(s => s.Plan)
                .Include(s => s.Payments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLower();
                query = query.Where(s =>
                    (s.User.Email != null && s.User.Email.ToLower().Contains(key)) ||
                    ((s.User.FirstName + " " + s.User.LastName).ToLower().Contains(key)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }

            if (planId.HasValue)
            {
                query = query.Where(s => s.PlanId == planId.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public Task<Subscription?> GetSubscriptionByIdWithDetailsAsync(int id)
        {
            return _db.Subscriptions
                .Include(s => s.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(s => s.Plan)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync()
        {
            return await _db.SubscriptionPlans
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }
    }
}
