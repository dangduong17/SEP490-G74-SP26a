using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly FindingJobsDbContext _context;

        public ProfileRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        // ── Candidate profile ──────────────────────────────────────────────────
        public async Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
                return null;

            var candidate = await _context.Candidates
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (candidate == null) return null;

            return new UserProfileDTO
            {
                CandidateId       = candidate.Id,
                UserId            = candidate.UserId.ToString(),
                FullName          = candidate.FullName ?? string.Empty,
                Email             = candidate.User?.Email ?? string.Empty,
                Phone             = candidate.Phone ?? string.Empty,
                AvatarUrl         = candidate.Avatar ?? string.Empty,
                City              = candidate.City ?? string.Empty,
                Address           = candidate.Address ?? string.Empty,
                Title             = candidate.Title ?? string.Empty,
                CurrentSalary     = candidate.CurrentSalary,
                ExpectedSalary    = candidate.ExpectedSalary,
                Summary           = candidate.Summary ?? string.Empty,
                YearsOfExperience = candidate.YearsOfExperience,
                WorkingType       = string.Empty,
                CurrentPosition   = string.Empty,
                HighestDegree     = string.Empty,
                IsLookingForJob   = candidate.IsLookingForJob ?? false,
                AllowContact      = false,
                CreatedAt         = candidate.CreatedAt ?? DateTime.MinValue,
                UpdatedAt         = null,
            };
        }

        // ── Password ───────────────────────────────────────────────────────────
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt    = DateTimeHelper.NowVietnam;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Recruiter profile ──────────────────────────────────────────────────
        public async Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId)
        {
            var recruiter = await _context.Recruiters
                .Include(r => r.User)
                .Include(r => r.Company)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null) return null;

            var user    = recruiter.User;
            var company = recruiter.Company;

            return new RecruiterProfileUpdateViewModel
            {
                RecruiterId        = recruiter.Id,
                CompanyId          = recruiter.CompanyId,
                FirstName          = user?.FirstName ?? string.Empty,
                LastName           = user?.LastName  ?? string.Empty,
                Phone              = recruiter.Phone    ?? string.Empty,
                Position           = recruiter.Position ?? string.Empty,
                Department         = null,

                CompanyName        = company?.Name        ?? string.Empty,
                CompanyTaxCode     = company?.TaxCode,
                CompanySize        = company?.CompanySize,
                CompanyIndustry    = company?.Industry,
                CompanyWebsite     = company?.Website,
                CompanyEmail       = company?.Email,
                CompanyPhone       = company?.Phone,
                CompanyDescription = company?.Description,

                ProvinceName = null,
                WardName     = null,
                WorkAddress  = null,
            };
        }

        public async Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model)
        {
            // 1. Update User
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName  = model.LastName;
            user.UpdatedAt = DateTimeHelper.NowVietnam;

            // 2. Update Recruiter row
            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return false;

            recruiter.FullName = $"{model.FirstName} {model.LastName}".Trim();
            recruiter.Phone    = model.Phone;
            recruiter.Position = model.Position;

            // 3. Upsert Company
            Company? company = recruiter.CompanyId.HasValue
                ? await _context.Companies.FirstOrDefaultAsync(c => c.Id == recruiter.CompanyId.Value)
                : null;

            if (company == null)
            {
                company = new Company { CreatedAt = DateTimeHelper.NowVietnam };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();   // flush to get new Id
                recruiter.CompanyId = company.Id;
            }

            company.Name        = model.CompanyName;
            company.TaxCode     = model.CompanyTaxCode;
            company.CompanySize = model.CompanySize;
            company.Industry    = model.CompanyIndustry;
            company.Website     = model.CompanyWebsite;
            company.Email       = model.CompanyEmail;
            company.Phone       = model.CompanyPhone;
            company.Description = model.CompanyDescription;
            company.UpdatedAt   = DateTimeHelper.NowVietnam;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Candidate Edit Profile ─────────────────────────────────────────────
        public async Task<CandidateEditProfileViewModel?> GetCandidateProfileForEditAsync(int userId)
        {
            var candidate = await _context.Candidates
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (candidate == null) return null;

            var user = candidate.User;
            return new CandidateEditProfileViewModel
            {
                UserId = userId,
                CandidateId = candidate.Id,
                Email = user?.Email ?? string.Empty,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                PhoneNumber = candidate.Phone ?? string.Empty,
                Title = candidate.Title,
                DateOfBirth = candidate.DateOfBirth,
                Gender = candidate.Gender,
                City = candidate.City,
                Address = candidate.Address,
                YearsOfExperience = candidate.YearsOfExperience,
                CurrentSalary = candidate.CurrentSalary,
                ExpectedSalary = candidate.ExpectedSalary,
                Summary = candidate.Summary,
                IsLookingForJob = candidate.IsLookingForJob ?? false,
                Avatar = candidate.Avatar
            };
        }

        public async Task<bool> UpdateCandidateProfileAsync(int userId, CandidateEditProfileViewModel model)
        {
            // 1. Update User (but not email)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            // Don't update email - it's readonly
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.PhoneNumber;
            user.UpdatedAt = DateTimeHelper.NowVietnam;

            // 2. Update Candidate
            var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) return false;

            candidate.FullName = $"{model.FirstName} {model.LastName}".Trim();
            candidate.Phone = model.PhoneNumber;
            candidate.Title = model.Title;
            candidate.DateOfBirth = model.DateOfBirth;
            candidate.Gender = model.Gender;
            candidate.City = model.City;
            candidate.Address = model.Address;
            candidate.YearsOfExperience = model.YearsOfExperience;
            candidate.CurrentSalary = model.CurrentSalary;
            candidate.ExpectedSalary = model.ExpectedSalary;
            candidate.Summary = model.Summary;
            candidate.IsLookingForJob = model.IsLookingForJob;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Recruiter New Edit Profile ─────────────────────────────────────────
        public async Task<RecruiterEditProfileViewModel?> GetRecruiterProfileForEditAsync(int userId)
        {
            var recruiter = await _context.Recruiters
                .Include(r => r.User)
                .Include(r => r.Company)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null) return null;

            var user = recruiter.User;
            var company = recruiter.Company;

            return new RecruiterEditProfileViewModel
            {
                UserId = userId,
                RecruiterId = recruiter.Id,
                CompanyId = recruiter.CompanyId,
                Email = user?.Email ?? string.Empty,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                PhoneNumber = recruiter.Phone ?? string.Empty,
                Position = recruiter.Position ?? string.Empty,
                Department = null, // Not stored in current schema
                Avatar = recruiter.Avatar,
                // Company fields
                CompanyName = company?.Name ?? string.Empty,
                CompanyTaxCode = company?.TaxCode,
                CompanySize = company?.CompanySize,
                CompanyIndustry = company?.Industry,
                CompanyWebsite = company?.Website,
                CompanyEmail = company?.Email,
                CompanyPhone = company?.Phone,
                CompanyDescription = company?.Description,
                ProvinceCode = company?.ProvinceCode,
                ProvinceName = company?.ProvinceName,
                WardCode = company?.WardCode,
                WardName = company?.WardName,
                WorkAddress = company?.Address,
                IsVerified = recruiter.IsVerified ?? false
            };
        }

        public async Task<bool> UpdateRecruiterProfileNewAsync(int userId, RecruiterEditProfileViewModel model)
        {
            // 1. Update User (but not email)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            // Don't update email - it's readonly
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.PhoneNumber;
            user.UpdatedAt = DateTimeHelper.NowVietnam;

            // 2. Update Recruiter
            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return false;

            recruiter.FullName = $"{model.FirstName} {model.LastName}".Trim();
            recruiter.Phone = model.PhoneNumber;
            recruiter.Position = model.Position;

            // 3. Update Company
            if (model.CompanyId.HasValue)
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == model.CompanyId.Value);
                if (company != null)
                {
                    company.Name = model.CompanyName;
                    company.TaxCode = model.CompanyTaxCode;
                    company.CompanySize = model.CompanySize;
                    company.Industry = model.CompanyIndustry;
                    company.Website = model.CompanyWebsite;
                    company.Email = model.CompanyEmail;
                    company.Phone = model.CompanyPhone ?? model.PhoneNumber;
                    company.Description = model.CompanyDescription;
                    company.ProvinceCode = model.ProvinceCode;
                    company.ProvinceName = model.ProvinceName;
                    company.WardCode = model.WardCode;
                    company.WardName = model.WardName;
                    company.Address = model.WorkAddress;
                    company.UpdatedAt = DateTimeHelper.NowVietnam;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Admin Edit Profile ──────────────────────────────────────────────────
        public async Task<AdminEditProfileViewModel?> GetAdminProfileForEditAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new AdminEditProfileViewModel
            {
                UserId = userId,
                Email = user.Email,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.Phone,
                Avatar = user.Avatar
            };
        }

        public async Task<bool> UpdateAdminProfileAsync(int userId, AdminEditProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.PhoneNumber;
            user.UpdatedAt = DateTimeHelper.NowVietnam;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
