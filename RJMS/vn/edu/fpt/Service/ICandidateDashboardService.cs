using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ICandidateDashboardService
    {
        Task<CandidateDashboardDTO> GetDashboardAsync(string userId);
    }
}
