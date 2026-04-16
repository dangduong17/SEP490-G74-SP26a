using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class RecruiterManagementRepository : IRecruiterManagementRepository
    {
        private readonly FindingJobsDbContext _db;
        public RecruiterManagementRepository(FindingJobsDbContext db) => _db = db;

        // ── Company resolution ─────────────────────────────────────────────

        public Task<Recruiter?> GetRecruiterByUserIdAsync(int userId) =>
            _db.Recruiters.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId);

        public Task<Company?> GetCompanyByIdAsync(int companyId) =>
            _db.Companies.FirstOrDefaultAsync(c => c.Id == companyId);

        // ── Company Locations ──────────────────────────────────────────────

        public Task<List<CompanyLocation>> GetCompanyLocationsAsync(int companyId) =>
            _db.CompanyLocations
               .Include(cl => cl.Location)
               .Include(cl => cl.RecruiterLocations)
               .Where(cl => cl.CompanyId == companyId)
               .OrderByDescending(cl => cl.IsPrimary)
               .ThenBy(cl => cl.CreatedAt)
               .ToListAsync();

        public Task<CompanyLocation?> GetCompanyLocationByIdAsync(int id) =>
            _db.CompanyLocations
               .Include(cl => cl.Location)
               .Include(cl => cl.RecruiterLocations)
               .FirstOrDefaultAsync(cl => cl.Id == id);

        public async Task AddCompanyLocationAsync(CompanyLocation location)
        {
            _db.CompanyLocations.Add(location);
            await _db.SaveChangesAsync();
        }

        public Task<Location?> GetMatchingLocationAsync(int? provinceCode, int? wardCode, string? address) =>
            _db.Locations.FirstOrDefaultAsync(l =>
                l.ProvinceCode == provinceCode &&
                l.WardCode == wardCode &&
                l.Address == address);

        public async Task AddLocationAsync(Location location)
        {
            _db.Locations.Add(location);
            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        // ── Employees ─────────────────────────────────────────────────────

        public async Task<(int total, List<Recruiter> items)> GetEmployeesPagedAsync(
            int companyId, string? keyword, int page, int pageSize)
        {
            // Employees = recruiters in this company whose user has "Employee" role
            var query = _db.Recruiters
                .Include(r => r.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(r => r.RecruiterLocations).ThenInclude(rl => rl.CompanyLocation).ThenInclude(cl => cl.Location)
                .Where(r => r.CompanyId == companyId &&
                            r.User.UserRoles.Any(ur => ur.Role.Name == "Employee"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(r =>
                    (r.FullName != null && r.FullName.ToLower().Contains(k)) ||
                    (r.User.Email != null && r.User.Email.ToLower().Contains(k)) ||
                    (r.Position != null && r.Position.ToLower().Contains(k)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public Task<Recruiter?> GetEmployeeByIdAsync(int recruiterId, int companyId) =>
            _db.Recruiters
               .Include(r => r.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
               .Include(r => r.RecruiterLocations)
               .FirstOrDefaultAsync(r => r.Id == recruiterId && r.CompanyId == companyId);

        public Task<User?> GetUserByIdAsync(int userId) =>
            _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        public Task<bool> UserEmailExistsAsync(string email) =>
            _db.Users.AnyAsync(u => u.Email == email);

        public async Task<User> CreateUserAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public Task<Role?> GetRoleByNameAsync(string roleName) =>
            _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        public async Task AssignRoleAsync(int userId, string roleName)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return;
            var exists = await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
            if (!exists)
            {
                _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
                await _db.SaveChangesAsync();
            }
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();
            return userRole;
        }

        public async Task<Recruiter> AddRecruiterAsync(Recruiter recruiter)
        {
            _db.Recruiters.Add(recruiter);
            await _db.SaveChangesAsync();
            return recruiter;
        }

        public async Task UpdateRecruiterAsync(Recruiter recruiter)
        {
            _db.Recruiters.Update(recruiter);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public Task<List<RecruiterLocation>> GetRecruiterLocationsAsync(int recruiterId) =>
            _db.RecruiterLocations.Where(rl => rl.RecruiterId == recruiterId).ToListAsync();

        public async Task AddRecruiterLocationAsync(RecruiterLocation rl)
        {
            _db.RecruiterLocations.Add(rl);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRecruiterLocationAsync(RecruiterLocation rl)
        {
            _db.RecruiterLocations.Remove(rl);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAllRecruiterLocationsAsync(int recruiterId)
        {
            var rls = await _db.RecruiterLocations.Where(rl => rl.RecruiterId == recruiterId).ToListAsync();
            _db.RecruiterLocations.RemoveRange(rls);
            await _db.SaveChangesAsync();
        }

        public async Task SetUserActiveAsync(int userId, bool isActive)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
