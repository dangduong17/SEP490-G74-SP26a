using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IJobService
    {
        Task<PublicJobListViewModel> GetPublicJobListAsync(string? keyword, int? categoryId, int? locationId, int page);
        Task<JobDetailViewModel?> GetJobDetailAsync(int id);
    }
}
