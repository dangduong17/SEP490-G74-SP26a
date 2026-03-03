using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        public JobApplicationRepository() { }

        public async Task<IReadOnlyCollection<JobApplicationDTO>> GetApplicationsAsync(Guid userId)
        {
            // TODO: replace with database access
            await Task.CompletedTask;

            return new List<JobApplicationDTO>
            {
                new JobApplicationDTO
                {
                    Id = Guid.NewGuid(),
                    PositionTitle = "Sample Position",
                    Status = "Pending",
                    AppliedAt = DateTime.UtcNow.AddDays(-5),
                },
                new JobApplicationDTO
                {
                    Id = Guid.NewGuid(),
                    PositionTitle = "Placeholder Role",
                    Status = "Reviewed",
                    AppliedAt = DateTime.UtcNow.AddDays(-12),
                },
            };
        }
    }
}
