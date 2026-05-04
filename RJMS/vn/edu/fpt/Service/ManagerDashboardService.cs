using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class ManagerDashboardService : IManagerDashboardService
    {
        private readonly IManagerRepository _repo;

        public ManagerDashboardService(IManagerRepository repo)
        {
            _repo = repo;
        }

        public async Task<ManagerDashboardViewModel> GetDashboardAsync()
        {
            var period7 = await _repo.GetDashboardPeriodDataAsync(7);
            var period30 = await _repo.GetDashboardPeriodDataAsync(30);
            var period90 = await _repo.GetDashboardPeriodDataAsync(90);

            return new ManagerDashboardViewModel
            {
                Periods = new Dictionary<int, ManagerPeriodData>
                {
                    [7] = period7,
                    [30] = period30,
                    [90] = period90
                },
                RecentActivities = await _repo.GetRecentActivitiesAsync(8),
                Alerts = await _repo.GetAlertItemsAsync()
            };
        }
        public async Task<ManagerPeriodData> GetDashboardRangeAsync(DateTime from, DateTime to)
        {
            return await _repo.GetDashboardRangeDataAsync(from, to);
        }
    }
}
