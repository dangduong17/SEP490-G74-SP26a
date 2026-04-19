using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface ISubscriptionRepository
    {
        Task<SubscriptionListViewModel> GetPlanListAsync(
            string? keyword, string? status, string? type, int page, int pageSize);

        Task<SubscriptionPlanDetailDto?> GetPlanDetailAsync(int id);

        Task<SubscriptionPlanFormViewModel?> GetPlanForEditAsync(int id);

        /// <summary>Get active plans grouped by base name (Basic/Pro/Enterprise) for display.</summary>
        Task<List<SubscriptionPlanGroupDto>> GetGroupedPlansForDisplayAsync();

        /// <summary>Insert one plan per BillingCycle checkbox selected.</summary>
        Task<List<int>> CreatePlansForCyclesAsync(SubscriptionPlanFormViewModel model);

        Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model);

        Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model);

        Task<bool> TogglePlanStatusAsync(int id);

        Task<bool> DeletePlanAsync(int id);

        // ── Period / Renewal ──
        Task<SubscriptionPeriodDto?> GetCurrentPeriodAsync(int subscriptionId);
        Task CreatePeriodAsync(int subscriptionId, int planId, DateTime start, DateTime end);

        /// <summary>Renew monthly periods for all active yearly subscriptions that have no current period.</summary>
        Task<int> RenewExpiredPeriodsAsync();

        /// <summary>Process subscriptions whose date range is no longer valid.</summary>
        Task<int> ProcessExpiredSubscriptionsAsync();

        // ── Quota ──
        Task<QuotaCheckResult> CheckQuotaAsync(int userId, string featureCode);
        Task ConsumeQuotaAsync(int userId, string featureCode);
    }
}
