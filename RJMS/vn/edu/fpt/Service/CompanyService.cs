using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CompanyService : ICompanyService
    {
        private readonly FindingJobsDbContext _context;

        public CompanyService(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyDetailsViewModel?> GetCompanyDetailsAsync(int id, int? currentUserId = null)
        {
            var company = await _context.Companies
                .Include(c => c.Jobs.Where(j => j.Status == "Active"))
                .ThenInclude(j => j.JobCategory)
                .Include(c => c.Jobs)
                .ThenInclude(j => j.JobRecruiters)
                .Include(c => c.CompanyLocations)
                .ThenInclude(cl => cl.Location)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return null;

            var followerCount = await _context.CompanyFollowers.CountAsync(f => f.CompanyId == id);
            var isFollowing = currentUserId.HasValue && await _context.CompanyFollowers.AnyAsync(f => f.CompanyId == id && f.UserId == currentUserId.Value);

            var model = new CompanyDetailsViewModel
            {
                Id = company.Id,
                Name = company.Name,
                Logo = company.Logo,
                CoverImage = company.CoverImage,
                Description = company.Description,
                Benefits = company.Benefits,
                CompanySize = company.CompanySize,
                Industry = company.Industry,
                Website = company.Website,
                Address = company.CompanyLocations
                    .FirstOrDefault(cl => cl.IsPrimary)?.Location?.Address
                    ?? company.CompanyLocations.FirstOrDefault()?.Location?.Address,
                IsVerified = company.IsVerified,
                FollowerCount = followerCount,
                IsFollowing = isFollowing,
                ActiveJobs = company.Jobs
                    .Where(j => j.Status == "Active")
                    .Select(j => new PublicJobListItemDTO
                    {
                        Id = j.Id,
                        Title = j.Title,
                        CompanyName = company.Name,
                        CompanyLogo = company.Logo,
                        LocationName = company.CompanyLocations.FirstOrDefault(cl => cl.IsPrimary)?.Location?.CityName
                            ?? company.CompanyLocations.FirstOrDefault()?.Location?.CityName,
                        MinSalary = j.MinSalary,
                        MaxSalary = j.MaxSalary,
                        CreatedAt = j.CreatedAt,
                        JobType = j.JobType,
                        CategoryName = j.JobCategory?.Name
                    }).ToList()
            };

            return model;
        }

        public async Task<bool> FollowCompanyAsync(int id, int userId)
        {
            var exists = await _context.CompanyFollowers.AnyAsync(f => f.CompanyId == id && f.UserId == userId);
            if (exists) return true;

            var follow = new CompanyFollower { CompanyId = id, UserId = userId };
            _context.CompanyFollowers.Add(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowCompanyAsync(int id, int userId)
        {
            var follow = await _context.CompanyFollowers.FirstOrDefaultAsync(f => f.CompanyId == id && f.UserId == userId);
            if (follow == null) return true;

            _context.CompanyFollowers.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
