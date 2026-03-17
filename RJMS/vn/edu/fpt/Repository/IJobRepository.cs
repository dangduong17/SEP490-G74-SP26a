using System.Collections.Generic;
using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IJobRepository
    {
        Task<(IEnumerable<Job> Jobs, int TotalCount)> GetPublicJobListAsync(string? keyword, int? categoryId, int? locationId, int page, int pageSize);
        Task<(List<JobFilterCategoryDTO> Categories, List<JobFilterLocationDTO> Locations)> GetFilterDataAsync();
        Task<Job?> GetJobDetailAsync(int id);
    }
}
