using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IJobApplicationRepository
    {
        Task<IReadOnlyCollection<JobApplicationDTO>> GetApplicationsAsync(string userId);
    }
}
