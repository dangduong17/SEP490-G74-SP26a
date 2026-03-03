using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class CandidateDashboardRepository : ICandidateDashboardRepository
    {
        public CandidateDashboardRepository() { }

        public async Task<CandidateDashboardDTO> GetCandidateDashboardAsync(Guid userId)
        {
            // TODO: replace with real data access
            await Task.CompletedTask;

            return new CandidateDashboardDTO
            {
                UserId = userId == Guid.Empty ? Guid.NewGuid() : userId,
                TotalApplications = 3,
                InterviewsScheduled = 1,
                OffersReceived = 0,
                LastUpdatedAt = DateTime.UtcNow,
            };
        }
    }
}
