using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly FindingJobsDbContext _context;

        public JobRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetPublicJobListAsync(
            string? keyword, int? categoryId, int? locationId, int page, int pageSize)
        {
            var query = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Location)
                .Include(j => j.JobCategory)
                .Where(j => j.Status == "Active")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(j => j.Title.Contains(keyword) || j.Company.Name.Contains(keyword));

            if (categoryId.HasValue)
            {
                var subCategoryIds = await _context.JobCategories
                    .Where(c => c.Id == categoryId.Value || c.ParentId == categoryId.Value || c.Parent!.ParentId == categoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();
                
                query = query.Where(j => j.JobCategoryId.HasValue && subCategoryIds.Contains(j.JobCategoryId.Value));
            }

            if (locationId.HasValue)
                query = query.Where(j => j.LocationId == locationId.Value);

            var totalCount = await query.CountAsync();
            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (jobs, totalCount);
        }

        public async Task<(List<JobFilterCategoryDTO> Categories, List<JobFilterLocationDTO> Locations)> GetFilterDataAsync()
        {
            var allCats = await _context.JobCategories
                .OrderBy(c => c.Level).ThenBy(c => c.Name)
                .ToListAsync();

            var jobCountMap = await _context.Jobs
                .Where(j => j.Status == "Active" && j.JobCategoryId != null)
                .GroupBy(j => j.JobCategoryId!.Value)
                .Select(g => new { CatId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CatId, x => x.Count);

            var flatCategories = allCats
                .Select(c => new JobFilterCategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Level = c.Level,
                    JobCount = jobCountMap.GetValueOrDefault(c.Id, 0)
                })
                .ToList();

            // Only locations that have active jobs
            var locations = await _context.Jobs
                .Where(j => j.Status == "Active" && j.LocationId != null)
                .Include(j => j.Location)
                .GroupBy(j => new { j.Location!.Id, j.Location.CityName })
                .Select(g => new JobFilterLocationDTO
                {
                    Id = g.Key.Id,
                    Name = g.Key.CityName,
                    JobCount = g.Count()
                })
                .OrderBy(l => l.Name)
                .ToListAsync();

            return (flatCategories, locations);
        }

        public async Task<Job?> GetJobDetailAsync(int id)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Location)
                .Include(j => j.JobCategory)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id);
        }
    }
}
