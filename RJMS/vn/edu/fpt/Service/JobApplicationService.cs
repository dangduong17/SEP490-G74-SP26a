using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _repository;

        public JobApplicationService(IJobApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyCollection<JobApplicationDTO>> GetApplicationsAsync(Guid userId)
        {
            return await _repository.GetApplicationsAsync(userId);
        }
    }
}
