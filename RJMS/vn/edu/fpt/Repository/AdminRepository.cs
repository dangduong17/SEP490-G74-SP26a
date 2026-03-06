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
    }
}
