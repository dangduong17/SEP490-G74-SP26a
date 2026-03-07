using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
<<<<<<< Updated upstream
using vn.edu.fpt.Utilities;
=======
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
>>>>>>> Stashed changes

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
                CandidateId        = candidate.Id,
                UserId             = candidate.UserId.ToString(),
                FullName           = candidate.FullName ?? string.Empty,
                Email              = candidate.User?.Email ?? string.Empty,
                Phone              = candidate.Phone ?? string.Empty,
                AvatarUrl          = candidate.Avatar ?? string.Empty,
                City               = candidate.City ?? string.Empty,
                Address            = candidate.Address ?? string.Empty,
                Title              = candidate.Title ?? string.Empty,
                CurrentSalary      = candidate.CurrentSalary,
                ExpectedSalary     = candidate.ExpectedSalary,
                Summary            = candidate.Summary ?? string.Empty,
                YearsOfExperience  = candidate.YearsOfExperience,
                WorkingType        = string.Empty,
                CurrentPosition    = string.Empty,
                HighestDegree      = string.Empty,
                IsLookingForJob    = candidate.IsLookingForJob ?? false,
                AllowContact       = false,
                CreatedAt          = candidate.CreatedAt ?? DateTime.MinValue,
                UpdatedAt          = null,
            };
        }

<<<<<<< Updated upstream
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTimeHelper.NowVietnam;
=======
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

            // Parse saved city string into ProvinceName / ProvinceName (stored as "ProvinceName|WardName" or plain)
            // Current schema saves city as plain text — we store ProvinceName there for simplicity
            return new RecruiterProfileUpdateViewModel
            {
                RecruiterId      = recruiter.Id,
                CompanyId        = recruiter.CompanyId,
                FirstName        = user?.FirstName ?? string.Empty,
                LastName         = user?.LastName  ?? string.Empty,
                Phone            = recruiter.Phone ?? string.Empty,
                Position         = recruiter.Position ?? string.Empty,
                Department       = null,

                CompanyName        = company?.Name        ?? string.Empty,
                CompanyTaxCode     = company?.TaxCode,
                CompanySize        = company?.CompanySize,
                CompanyIndustry    = company?.Industry,
                CompanyWebsite     = company?.Website,
                CompanyEmail       = company?.Email,
                CompanyPhone       = company?.Phone,
                CompanyDescription = company?.Description,

                // Location: ProvinceName is stored in Company.Description prefix or we leave blank for now
                ProvinceName = null,
                WardName     = null,
                WorkAddress  = null,
            };
        }

        public async Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model)
        {
            // 1. Update User (FirstName, LastName)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName  = model.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            // 2. Update Recruiter row
            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return false;

            recruiter.FullName = $"{model.FirstName} {model.LastName}".Trim();
            recruiter.Phone    = model.Phone;
            recruiter.Position = model.Position;

            // 3. Upsert Company
            Company? company;
            if (recruiter.CompanyId.HasValue)
            {
                company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == recruiter.CompanyId.Value);
            }
            else
            {
                company = null;
            }

            if (company == null)
            {
                // Create new company and link to recruiter
                company = new Company
                {
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();          // get new company Id
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
            company.UpdatedAt   = DateTime.UtcNow;
>>>>>>> Stashed changes

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
