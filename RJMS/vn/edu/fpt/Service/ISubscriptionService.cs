using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ISubscriptionService
    {
        Task<SubscriptionListViewModel> GetPlanListAsync(
            string? keyword, string? status, string? type, int page, int pageSize);

        Task<SubscriptionPlanDetailDto?> GetPlanDetailAsync(int id);

        Task<SubscriptionPlanFormViewModel?> GetPlanForEditAsync(int id);

        /// <summary>Get grouped plans for UI display (Monthly + Yearly variants).</summary>
        Task<List<SubscriptionPlanGroupDto>> GetGroupedPlansForDisplayAsync();

        /// <summary>Create 1 or 2 plans depending on BillingCycles checkbox.</summary>
        Task<List<int>> CreatePlansForCyclesAsync(SubscriptionPlanFormViewModel model);

        Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model);

        Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model);

        Task<bool> TogglePlanStatusAsync(int id);

        Task<bool> DeletePlanAsync(int id);

        // ── Period / Quota ──
        Task<SubscriptionPeriodDto?> GetCurrentPeriodAsync(int subscriptionId);
        Task<QuotaCheckResult> CheckQuotaAsync(int userId, string featureCode);
        Task ConsumeQuotaAsync(int userId, string featureCode);
        Task<int> RenewExpiredPeriodsAsync();
        Task<int> ProcessExpiredSubscriptionsAsync();

        // ── Cancel ──
        Task<bool> CancelSubscriptionAsync(int subscriptionId);

        // ── History ──
        Task<RecruiterSubscriptionHistoryViewModel> GetRecruiterHistoryAsync(int userId, int page, int pageSize);
    }
}
