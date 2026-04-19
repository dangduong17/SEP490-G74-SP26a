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
        Task<int?> GetCandidateIdByUserIdAsync(int userId);
        Task<HashSet<int>> GetSavedJobIdsAsync(int candidateId, IEnumerable<int> jobIds);
        Task<bool> ToggleSavedJobAsync(int candidateId, int jobId);
        Task<bool> IsJobSavedAsync(int candidateId, int jobId);
        Task<(IEnumerable<Job> Jobs, int TotalCount)> GetSavedJobListAsync(int candidateId, int page, int pageSize);
    }
}
