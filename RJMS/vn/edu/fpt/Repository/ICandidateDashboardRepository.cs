using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface ICandidateDashboardRepository
    {
        Task<CandidateDashboardDTO> GetCandidateDashboardAsync(string userId);
    }
}
