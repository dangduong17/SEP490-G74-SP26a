using Microsoft.EntityFrameworkCore;
using RJMS.Models;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly G74FindingJobsContext _context;

        public ProfileRepository(G74FindingJobsContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var candidate = await _context
                .Candidates.Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (candidate == null)
            {
                return null;
            }

            return new UserProfileDTO
            {
                CandidateId = candidate.Id,
                UserId = candidate.UserId,
                FullName = candidate.FullName ?? string.Empty,
                Email = candidate.User?.Email ?? string.Empty,
                Phone = candidate.Phone ?? string.Empty,
                AvatarUrl = candidate.Avatar ?? string.Empty,
                City = candidate.City ?? string.Empty,
                District = candidate.District ?? string.Empty,
                Address = candidate.Address ?? string.Empty,
                Title = candidate.Title ?? string.Empty,
                CurrentSalary = candidate.CurrentSalary,
                ExpectedSalary = candidate.ExpectedSalary,
                WorkingType = candidate.WorkingType ?? string.Empty,
                Summary = candidate.Summary ?? string.Empty,
                CurrentPosition = candidate.CurrentPosition ?? string.Empty,
                YearsOfExperience = candidate.YearsOfExperience,
                HighestDegree = candidate.HighestDegree ?? string.Empty,
                IsLookingForJob = candidate.IsLookingForJob,
                AllowContact = candidate.AllowContact,
                CreatedAt = candidate.CreatedAt,
                UpdatedAt = candidate.UpdatedAt,
            };
        }
    }
}
