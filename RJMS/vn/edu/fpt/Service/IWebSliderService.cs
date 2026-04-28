using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IWebSliderService
    {
        Task<WebSliderListViewModel> GetListAsync(string? keyword, string? statusFilter, int page, int pageSize);
        Task<List<WebSliderDto>> GetActiveForDisplayAsync();
        Task<WebSliderFormViewModel?> GetForEditAsync(int id);
        Task<(bool success, string message)> CreateAsync(WebSliderFormViewModel form);
        Task<(bool success, string message)> UpdateAsync(WebSliderFormViewModel form);
        Task<(bool success, string message)> DeleteAsync(int id);
        Task<int> ExpireOutdatedAsync();
    }
}
