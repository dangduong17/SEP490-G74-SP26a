using System;
using Microsoft.EntityFrameworkCore;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly FindingJobsDbContext _context;

        public ProfileRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDTO?> GetProfileByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            if (!int.TryParse(userId, out var userIdValue))
            {
                return null;
            }

            var candidate = await _context
                .Candidates.Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userIdValue);

            if (candidate == null)
            {
                return null;
            }

            return new UserProfileDTO
            {
                CandidateId = candidate.Id,
                UserId = candidate.UserId.ToString(),
                FullName = candidate.FullName ?? string.Empty,
                Email = candidate.User?.Email ?? string.Empty,
                Phone = candidate.Phone ?? string.Empty,
                AvatarUrl = candidate.Avatar ?? string.Empty,
                City = candidate.City ?? string.Empty,
                Address = candidate.Address ?? string.Empty,
                Title = candidate.Title ?? string.Empty,
                CurrentSalary = candidate.CurrentSalary,
                ExpectedSalary = candidate.ExpectedSalary,
                Summary = candidate.Summary ?? string.Empty,
                YearsOfExperience = candidate.YearsOfExperience,
                WorkingType = string.Empty,
                CurrentPosition = string.Empty,
                HighestDegree = string.Empty,
                IsLookingForJob = candidate.IsLookingForJob ?? false,
                AllowContact = false,
                CreatedAt = candidate.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = null,
            };
        }
    }
}
