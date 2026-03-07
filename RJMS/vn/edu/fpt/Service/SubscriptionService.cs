using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    /// <summary>
    /// Stub implementation — delegates straight to repository.
    /// Business logic (validation, email alerts, etc.) sẽ được thêm sau.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repo;

        public SubscriptionService(ISubscriptionRepository repo)
        {
            _repo = repo;
        }

        public Task<SubscriptionListViewModel> GetPlanListAsync(
            string? keyword, string? status, string? type, int page, int pageSize)
            => _repo.GetPlanListAsync(keyword, status, type, page, pageSize);

        public Task<SubscriptionPlanDetailDto?> GetPlanDetailAsync(int id)
            => _repo.GetPlanDetailAsync(id);

        public Task<SubscriptionPlanFormViewModel?> GetPlanForEditAsync(int id)
            => _repo.GetPlanForEditAsync(id);

        public Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model)
            => _repo.CreatePlanAsync(model);

        public Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model)
            => _repo.UpdatePlanAsync(model);

        public Task<bool> TogglePlanStatusAsync(int id)
            => _repo.TogglePlanStatusAsync(id);

        public Task<bool> DeletePlanAsync(int id)
            => _repo.DeletePlanAsync(id);
    }
}
