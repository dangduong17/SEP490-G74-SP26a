using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    /// <summary>
    /// Stub implementation — logic sẽ được implement ở sprint sau.
    /// Hiện tại trả về dữ liệu mẫu để frontend render được.
    /// </summary>
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly FindingJobsDbContext _context;

        public SubscriptionRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public Task<SubscriptionListViewModel> GetPlanListAsync(
            string? keyword, string? status, string? type, int page, int pageSize)
        {
            // TODO: query _context.SubscriptionPlans with filters + pagination
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanDetailDto?> GetPlanDetailAsync(int id)
        {
            // TODO: query plan + count active subscriptions + recent subscribers
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanFormViewModel?> GetPlanForEditAsync(int id)
        {
            // TODO: map SubscriptionPlan entity → SubscriptionPlanFormViewModel
            throw new NotImplementedException();
        }

        public Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model)
        {
            // TODO: insert new SubscriptionPlan, return new Id
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model)
        {
            // TODO: find by Id, apply field updates, SaveChanges
            throw new NotImplementedException();
        }

        public Task<bool> TogglePlanStatusAsync(int id)
        {
            // TODO: flip IsActive flag
            throw new NotImplementedException();
        }

        public Task<bool> DeletePlanAsync(int id)
        {
            // TODO: soft-delete or remove plan if no active subscriptions
            throw new NotImplementedException();
        }
    }
}
