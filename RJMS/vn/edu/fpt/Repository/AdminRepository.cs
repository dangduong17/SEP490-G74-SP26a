using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using vn.edu.fpt.Utilities;

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
                .Include(u => u.Recruiters).ThenInclude(r => r.Company)
                .Include(u => u.Recruiters).ThenInclude(r => r.RecruiterLocations)
                    .ThenInclude(rl => rl.CompanyLocation).ThenInclude(cl => cl.Location)
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

        public async Task<DashboardPeriodData> GetDashboardPeriodDataAsync(int days)
        {
            var endDate = DateTimeHelper.NowVietnam.Date;
            var startDate = endDate.AddDays(-(days - 1));
            var endExclusive = endDate.AddDays(1);

            var roleEvents = await _db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.User.CreatedAt != null && ur.User.CreatedAt >= startDate && ur.User.CreatedAt < endExclusive)
                .Where(ur => ur.Role.Name == "Candidate" || ur.Role.Name == "Recruiter")
                .Select(ur => new RoleEvent(ur.Role.Name, ur.User.CreatedAt!.Value))
                .ToListAsync();

            var jobEvents = await _db.Jobs
                .AsNoTracking()
                .Where(j => j.CreatedAt != null && j.CreatedAt >= startDate && j.CreatedAt < endExclusive)
                .Select(j => j.CreatedAt!.Value)
                .ToListAsync();

            var lockedJobs = await _db.Jobs
                .AsNoTracking()
                .Where(j => j.IsBanned && (j.BannedAt == null || (j.BannedAt >= startDate && j.BannedAt < endExclusive)))
                .CountAsync();

            var canceledSubs = await _db.Subscriptions
                .AsNoTracking()
                .Where(s => s.CancelledAt != null && s.CancelledAt >= startDate && s.CancelledAt < endExclusive)
                .CountAsync();

            var bannedSubs = await _db.Subscriptions
                .AsNoTracking()
                .Where(s => s.IsBanned && (s.BannedAt == null || (s.BannedAt >= startDate && s.BannedAt < endExclusive)))
                .CountAsync();

            var templateCategoryCounts = await _db.CvTemplates
                .AsNoTracking()
                .GroupBy(t => t.Category != null ? t.Category.Name : "Chưa phân loại")
                .Select(g => new DashboardCategoryCount { Label = g.Key ?? "Chưa phân loại", Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            var templateUsageCounts = await _db.Cvs
                .AsNoTracking()
                .Where(c => c.TemplateId != null)
                .Join(_db.CvTemplates.AsNoTracking(), c => c.TemplateId, t => t.Id, (c, t) => new { t.Name })
                .GroupBy(x => x.Name)
                .Select(g => new DashboardCategoryCount { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToListAsync();

            templateCategoryCounts = NormalizeCounts(templateCategoryCounts, "Chưa có dữ liệu");
            templateUsageCounts = NormalizeCounts(templateUsageCounts, "Chưa có dữ liệu");

            var data = new DashboardPeriodData
            {
                LockedJobs = lockedJobs,
                CanceledSubscriptions = canceledSubs,
                BannedSubscriptions = bannedSubs,
                TemplateCategoryCounts = templateCategoryCounts,
                TemplateUsageCounts = templateUsageCounts
            };

            if (days <= 7)
            {
                BuildDailySeries(data, startDate, days, roleEvents, jobEvents);
            }
            else if (days <= 30)
            {
                BuildWeeklySeries(data, startDate, 4, roleEvents, jobEvents);
            }
            else
            {
                BuildMonthlySeries(data, startDate, 3, roleEvents, jobEvents);
            }

            data.CandidateTotal = data.CandidateCounts.Sum();
            data.RecruiterTotal = data.RecruiterCounts.Sum();
            return data;
        }

        private static List<DashboardCategoryCount> NormalizeCounts(List<DashboardCategoryCount> items, string fallbackLabel)
        {
            if (items.Count > 0) return items;
            return new List<DashboardCategoryCount>
            {
                new DashboardCategoryCount { Label = fallbackLabel, Value = 0 }
            };
        }

        private static void BuildDailySeries(
            DashboardPeriodData data,
            DateTime startDate,
            int days,
            List<RoleEvent> roleEvents,
            List<DateTime> jobEvents)
        {
            var labels = new List<string>();
            var candidateCounts = new List<int>();
            var recruiterCounts = new List<int>();
            var jobCounts = new List<int>();

            for (var i = 0; i < days; i++)
            {
                var day = startDate.AddDays(i).Date;
                labels.Add(day.ToString("dd/MM"));
                candidateCounts.Add(roleEvents.Count(e => e.Role == "Candidate" && e.CreatedAt.Date == day));
                recruiterCounts.Add(roleEvents.Count(e => e.Role == "Recruiter" && e.CreatedAt.Date == day));
                jobCounts.Add(jobEvents.Count(d => d.Date == day));
            }

            data.Labels = labels;
            data.CandidateCounts = candidateCounts;
            data.RecruiterCounts = recruiterCounts;
            data.JobPostingCounts = jobCounts;
        }

        private static void BuildWeeklySeries(
            DashboardPeriodData data,
            DateTime startDate,
            int weeks,
            List<RoleEvent> roleEvents,
            List<DateTime> jobEvents)
        {
            var labels = Enumerable.Range(1, weeks).Select(i => $"Tuần {i}").ToList();
            var candidateCounts = Enumerable.Repeat(0, weeks).ToList();
            var recruiterCounts = Enumerable.Repeat(0, weeks).ToList();
            var jobCounts = Enumerable.Repeat(0, weeks).ToList();

            foreach (var ev in roleEvents)
            {
                var index = Math.Min(weeks - 1, (ev.CreatedAt.Date - startDate).Days / 7);
                if (index < 0) continue;
                if (ev.Role == "Candidate") candidateCounts[index]++;
                if (ev.Role == "Recruiter") recruiterCounts[index]++;
            }

            foreach (var jobDate in jobEvents)
            {
                var index = Math.Min(weeks - 1, (jobDate.Date - startDate).Days / 7);
                if (index < 0) continue;
                jobCounts[index]++;
            }

            data.Labels = labels;
            data.CandidateCounts = candidateCounts;
            data.RecruiterCounts = recruiterCounts;
            data.JobPostingCounts = jobCounts;
        }

        private static void BuildMonthlySeries(
            DashboardPeriodData data,
            DateTime startDate,
            int months,
            List<RoleEvent> roleEvents,
            List<DateTime> jobEvents)
        {
            var labels = Enumerable.Range(0, months)
                .Select(i => $"Tháng {startDate.AddMonths(i).Month}")
                .ToList();
            var candidateCounts = Enumerable.Repeat(0, months).ToList();
            var recruiterCounts = Enumerable.Repeat(0, months).ToList();
            var jobCounts = Enumerable.Repeat(0, months).ToList();

            foreach (var ev in roleEvents)
            {
                var index = (ev.CreatedAt.Year - startDate.Year) * 12 + ev.CreatedAt.Month - startDate.Month;
                if (index < 0 || index >= months) continue;
                if (ev.Role == "Candidate") candidateCounts[index]++;
                if (ev.Role == "Recruiter") recruiterCounts[index]++;
            }

            foreach (var jobDate in jobEvents)
            {
                var index = (jobDate.Year - startDate.Year) * 12 + jobDate.Month - startDate.Month;
                if (index < 0 || index >= months) continue;
                jobCounts[index]++;
            }

            data.Labels = labels;
            data.CandidateCounts = candidateCounts;
            data.RecruiterCounts = recruiterCounts;
            data.JobPostingCounts = jobCounts;
        }

        private sealed class RoleEvent
        {
            public RoleEvent(string role, DateTime createdAt)
            {
                Role = role;
                CreatedAt = createdAt;
            }

            public string Role { get; }
            public DateTime CreatedAt { get; }
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

        // ── Location ──────────────────────────────────────────────────────────
        public async Task<Location?> GetLocationAsync(int? provinceCode, int? wardCode, string? address)
        {
            return await _db.Locations.FirstOrDefaultAsync(l => 
                l.ProvinceCode == provinceCode && 
                l.WardCode == wardCode && 
                l.Address == address);
        }

        public async Task AddLocationAsync(Location location)
        {
            _db.Locations.Add(location);
            await _db.SaveChangesAsync();
        }

        public async Task AddCompanyLocationAsync(CompanyLocation companyLocation)
        {
            _db.CompanyLocations.Add(companyLocation);
            await _db.SaveChangesAsync();
        }

        public async Task AddRecruiterLocationAsync(RecruiterLocation recruiterLocation)
        {
            _db.RecruiterLocations.Add(recruiterLocation);
            await _db.SaveChangesAsync();
        }

        public Task<List<RecruiterLocation>> GetRecruiterLocationsByRecruiterIdAsync(int recruiterId)
        {
            return _db.RecruiterLocations
                .Include(rl => rl.CompanyLocation).ThenInclude(cl => cl.Location)
                .Where(rl => rl.RecruiterId == recruiterId)
                .ToListAsync();
        }

        public async Task RemoveRecruiterLocationAsync(RecruiterLocation recruiterLocation)
        {
            _db.RecruiterLocations.Remove(recruiterLocation);
            await _db.SaveChangesAsync();
        }

        public Task<List<CompanyLocation>> GetCompanyLocationsAsync(int companyId)
        {
            return _db.CompanyLocations
                .Include(cl => cl.Location)
                .Include(cl => cl.RecruiterLocations)
                .Where(cl => cl.CompanyId == companyId)
                .OrderByDescending(cl => cl.IsPrimary)
                .ThenBy(cl => cl.AddressLabel)
                .ToListAsync();
        }

        public Task<CompanyLocation?> GetCompanyLocationByIdAsync(int id)
        {
            return _db.CompanyLocations
                .Include(cl => cl.Location)
                .Include(cl => cl.RecruiterLocations)
                .FirstOrDefaultAsync(cl => cl.Id == id);
        }

        public async Task DeleteCompanyLocationAsync(CompanyLocation companyLocation)
        {
            _db.CompanyLocations.Remove(companyLocation);
            await _db.SaveChangesAsync();
        }

        // ── Employee ──────────────────────────────────────────────────────────
        public async Task<(int total, List<Recruiter> items)> GetEmployeesPagedAsync(string? keyword, int? companyId, int page, int pageSize)
        {
            // Employees = Recruiters whose User has role "Employee"
            var query = _db.Recruiters
                .Include(r => r.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(r => r.Company)
                .Include(r => r.RecruiterLocations).ThenInclude(rl => rl.CompanyLocation).ThenInclude(cl => cl.Location)
                .Where(r => r.User.UserRoles.Any(ur => ur.Role.Name == "Employee"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLower();
                query = query.Where(r =>
                    (r.FullName != null && r.FullName.ToLower().Contains(key)) ||
                    (r.User.Email != null && r.User.Email.ToLower().Contains(key)));
            }

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public Task<List<RecruiterLocation>> GetRecruiterLocationsAsync(int recruiterId)
        {
            return _db.RecruiterLocations
                .Include(rl => rl.CompanyLocation).ThenInclude(cl => cl.Location)
                .Where(rl => rl.RecruiterId == recruiterId)
                .ToListAsync();
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
                .Include(c => c.Followers)
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
                .Include(c => c.CompanyLocations).ThenInclude(cl => cl.Location)
                .Include(c => c.Followers)
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

        public async Task<List<string>> GetSubscriptionStatusesAsync()
        {
            return await _db.Subscriptions
                .Where(s => s.Status != null)
                .Select(s => s.Status!)
                .Distinct()
                .ToListAsync();
        }

        public Task<Subscription?> GetSubscriptionByIdAsync(int id)
        {
            return _db.Subscriptions.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSubscriptionAsync(Subscription subscription)
        {
            _db.Subscriptions.Update(subscription);
            await _db.SaveChangesAsync();
        }

        // ── Jobs ─────────────────────────────────────────────────────────────

        public IQueryable<Job> GetJobsQuery()
        {
            return _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Applications)
                .AsQueryable();
        }

        public Task<Job?> GetJobByIdAsync(int id)
        {
            return _db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        }

        public Task<Job?> GetJobByIdWithDetailsAsync(int id)
        {
            return _db.Jobs
                .Include(j => j.Company)
                .Include(j => j.Applications)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.JobRecruiters).ThenInclude(jr => jr.Recruiter).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task UpdateJobAsync(Job job)
        {
            _db.Jobs.Update(job);
            await _db.SaveChangesAsync();
        }
    }
}
