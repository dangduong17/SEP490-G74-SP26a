using System.Linq;
using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<PublicJobListViewModel> GetPublicJobListAsync(string? keyword, int? categoryId, int? locationId, int page)
        {
            const int pageSize = 10;
            var (jobs, totalCount) = await _jobRepository.GetPublicJobListAsync(keyword, categoryId, locationId, page, pageSize);
            var (categories, locations) = await _jobRepository.GetFilterDataAsync();

            var jobDtos = jobs.Select(j => new PublicJobListItemDTO
            {
                Id = j.Id,
                Title = j.Title,
                CompanyName = j.Company?.Name ?? "N/A",
                CompanyLogo = j.Company?.Logo,
                LocationName = j.Company?.CompanyLocations
                    .FirstOrDefault(cl => cl.IsPrimary)?.Location?.CityName
                    ?? j.Company?.CompanyLocations.FirstOrDefault()?.Location?.CityName,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                CreatedAt = j.CreatedAt,
                JobType = j.JobType,
                CategoryName = j.JobCategory?.Name
            }).ToList();

            return new PublicJobListViewModel
            {
                Jobs = jobDtos,
                TotalItems = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                Keyword = keyword,
                CategoryId = categoryId,
                LocationId = locationId,
                Categories = categories,
                CategoryGroups = BuildCategoryGroups(categories),
                Locations = locations
            };
        }

        public async Task<JobDetailViewModel?> GetJobDetailAsync(int id)
        {
            var job = await _jobRepository.GetJobDetailAsync(id);
            if (job == null) return null;

            return new JobDetailViewModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                JobType = job.JobType,
                MinSalary = job.MinSalary,
                MaxSalary = job.MaxSalary,
                NumberOfPositions = job.NumberOfPositions,
                ApplicationDeadline = job.ApplicationDeadline,
                ExpiryDate = job.ExpiryDate,
                CreatedAt = job.CreatedAt,
                Status = job.Status,
                // Location resolved from JobRecruiters -> CompanyLocation -> Location
                LocationName = job.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.CompanyLocation?.Location?.CityName
                    ?? job.Company?.CompanyLocations.FirstOrDefault(cl => cl.IsPrimary)?.Location?.CityName
                    ?? job.Company?.CompanyLocations.FirstOrDefault()?.Location?.CityName,
                LocationWardName = job.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.CompanyLocation?.Location?.WardName
                    ?? job.Company?.CompanyLocations.FirstOrDefault()?.Location?.WardName,
                LocationProvinceName = null,
                LocationAddress = job.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.CompanyLocation?.Location?.Address
                    ?? job.Company?.CompanyLocations.FirstOrDefault()?.Location?.Address,
                CategoryName = job.JobCategory?.Name,
                Skills = job.JobSkills?.Where(s => s.Skill != null).Select(s => s.Skill.Name).ToList() ?? new(),
                CompanyId = job.CompanyId,
                CompanyName = job.Company?.Name ?? "N/A",
                CompanyLogo = job.Company?.Logo,
                CompanyDescription = job.Company?.Description,
                CompanySize = job.Company?.CompanySize,
                CompanyIndustry = job.Company?.Industry,
                CompanyWebsite = job.Company?.Website,
                // Address now sourced from primary CompanyLocation
                CompanyAddress = job.Company?.CompanyLocations
                    .FirstOrDefault(cl => cl.IsPrimary)?.Location?.Address
                    ?? job.Company?.CompanyLocations.FirstOrDefault()?.Location?.Address,
                CompanyProvince = job.Company?.CompanyLocations
                    .FirstOrDefault(cl => cl.IsPrimary)?.Location?.CityName
                    ?? job.Company?.CompanyLocations.FirstOrDefault()?.Location?.CityName,
            };
        }

        private static List<JobFilterCategoryDTO> BuildCategoryGroups(List<JobFilterCategoryDTO> flat)
        {
            var lookup = flat.ToDictionary(c => c.Id, c => new JobFilterCategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                Level = c.Level,
                JobCount = c.JobCount
            });
            
            var roots = new List<JobFilterCategoryDTO>();
            
            // Build tree
            foreach (var cat in flat)
            {
                if (cat.ParentId == null || !lookup.ContainsKey(cat.ParentId.Value))
                    roots.Add(lookup[cat.Id]);
                else
                    lookup[cat.ParentId.Value].Children.Add(lookup[cat.Id]);
            }
            
            // Bubble up counts from leaves to roots
            void BubbleUp(JobFilterCategoryDTO node)
            {
                foreach (var child in node.Children)
                {
                    BubbleUp(child);
                }
                if (node.Children.Any())
                {
                    node.JobCount += node.Children.Sum(c => c.JobCount);
                }
            }

            foreach (var r in roots)
            {
                BubbleUp(r);
            }

            // Prune branches with 0 count
            void Prune(JobFilterCategoryDTO node)
            {
                node.Children = node.Children.Where(c => c.JobCount > 0).ToList();
                foreach (var child in node.Children)
                {
                    Prune(child);
                }
            }

            foreach (var r in roots)
            {
                Prune(r);
            }

            return roots.Where(r => r.JobCount > 0).ToList();
        }
    }
}
