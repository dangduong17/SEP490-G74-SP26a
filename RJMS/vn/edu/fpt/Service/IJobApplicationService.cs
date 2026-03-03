using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IJobApplicationService
    {
        Task<IReadOnlyCollection<JobApplicationDTO>> GetApplicationsAsync(Guid userId);
    }
}
