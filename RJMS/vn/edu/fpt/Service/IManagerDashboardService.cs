using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IManagerDashboardService
    {
        Task<ManagerDashboardViewModel> GetDashboardAsync();
        Task<ManagerPeriodData> GetDashboardRangeAsync(DateTime from, DateTime to);
    }
}
