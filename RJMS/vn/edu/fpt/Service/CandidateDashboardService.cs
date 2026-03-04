using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CandidateDashboardService : ICandidateDashboardService
    {
        private readonly ICandidateDashboardRepository _repository;

        public CandidateDashboardService(ICandidateDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<CandidateDashboardDTO> GetDashboardAsync(string userId)
        {
            return await _repository.GetCandidateDashboardAsync(userId);
        }
    }
}
