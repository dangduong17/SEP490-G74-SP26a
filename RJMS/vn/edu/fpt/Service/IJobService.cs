using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IJobService
    {
        Task<PublicJobListViewModel> GetPublicJobListAsync(string? keyword, int? categoryId, int? locationId, int page, int? currentUserId = null);
        Task<JobDetailViewModel?> GetJobDetailAsync(int id);
        Task<bool> ToggleSavedJobAsync(int currentUserId, int jobId);
        // Expose IsJobSaved check in service interface
        Task<bool> IsJobSavedAsync(int currentUserId, int jobId);
        Task<PublicJobListViewModel> GetSavedJobListAsync(int currentUserId, int page);
    }
}
