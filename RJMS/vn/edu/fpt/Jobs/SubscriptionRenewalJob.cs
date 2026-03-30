using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;

namespace RJMS.vn.edu.fpt.Jobs
{
    public class SubscriptionRenewalJob
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionRenewalJob(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// This method will be scheduled by Hangfire to run every night (or more frequently).
        /// It will automatically create a new period for each active yearly subscription 
        /// when at the start of a month.
        /// </summary>
        public async Task Execute()
        {
            // 1. Mark expired memberships
            await _subscriptionService.ProcessExpiredSubscriptionsAsync();

            // 2. Proactive month renewal for yearly subs
            await _subscriptionService.RenewExpiredPeriodsAsync();
        }
    }
}
