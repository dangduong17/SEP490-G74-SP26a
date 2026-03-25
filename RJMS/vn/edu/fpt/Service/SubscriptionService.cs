using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    /// <summary>
    /// Delegates to repository. Business logic will be layered here.
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

        public Task<List<int>> CreatePlansForCyclesAsync(SubscriptionPlanFormViewModel model)
            => _repo.CreatePlansForCyclesAsync(model);

        public Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model)
            => _repo.CreatePlanAsync(model);

        public Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model)
            => _repo.UpdatePlanAsync(model);

        public Task<bool> TogglePlanStatusAsync(int id)
            => _repo.TogglePlanStatusAsync(id);

        public Task<bool> DeletePlanAsync(int id)
            => _repo.DeletePlanAsync(id);

        // Period / Quota
        public Task<SubscriptionPeriodDto?> GetCurrentPeriodAsync(int subscriptionId)
            => _repo.GetCurrentPeriodAsync(subscriptionId);

        public Task<QuotaCheckResult> CheckQuotaAsync(int userId, string featureCode)
            => _repo.CheckQuotaAsync(userId, featureCode);

        public Task ConsumeQuotaAsync(int userId, string featureCode)
            => _repo.ConsumeQuotaAsync(userId, featureCode);

        public Task<int> RenewExpiredPeriodsAsync()
            => _repo.RenewExpiredPeriodsAsync();
    }
}
