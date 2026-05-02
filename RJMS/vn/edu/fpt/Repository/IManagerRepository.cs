using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IManagerRepository
    {
        Task<ManagerPeriodData> GetDashboardPeriodDataAsync(int days);
        Task<List<ManagerActivityItem>> GetRecentActivitiesAsync(int take);
        Task<List<ManagerAlertItem>> GetAlertItemsAsync();
    }
}
