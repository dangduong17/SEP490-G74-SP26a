using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ICompanyService
    {
        Task<CompanyListViewModel> GetCompanyListAsync(string? keyword, string? industry, int page, int pageSize);
        Task<CompanyDetailsViewModel?> GetCompanyDetailsAsync(int id, int? currentUserId = null);
        Task<bool> FollowCompanyAsync(int id, int userId);
        Task<bool> UnfollowCompanyAsync(int id, int userId);
    }
}
