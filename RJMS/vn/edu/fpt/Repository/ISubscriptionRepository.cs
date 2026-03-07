using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface ISubscriptionRepository
    {
        Task<SubscriptionListViewModel> GetPlanListAsync(
            string? keyword, string? status, string? type, int page, int pageSize);

        Task<SubscriptionPlanDetailDto?> GetPlanDetailAsync(int id);

        Task<SubscriptionPlanFormViewModel?> GetPlanForEditAsync(int id);

        Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model);

        Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model);

        /// <summary>Toggle IsActive flag.</summary>
        Task<bool> TogglePlanStatusAsync(int id);

        Task<bool> DeletePlanAsync(int id);
    }
}
