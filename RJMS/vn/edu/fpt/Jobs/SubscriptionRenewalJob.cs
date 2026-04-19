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
        /// This method will be scheduled by Hangfire to run daily (or more frequently).
        /// It performs automatic subscription renewal for monthly plans and period renewal for yearly plans.
        /// </summary>
        public async Task Execute()
        {
            // 1. Mark expired memberships (EndDate has passed)
            var expiredCount = await _subscriptionService.ProcessExpiredSubscriptionsAsync();
            if (expiredCount > 0)
            {
                System.Console.WriteLine($"[SubscriptionRenewalJob] Marked {expiredCount} subscriptions as expired.");
            }

            // 2. Create monthly periods for yearly subscriptions (proactive month renewal)
            var renewedCount = await _subscriptionService.RenewExpiredPeriodsAsync();
            if (renewedCount > 0)
            {
                System.Console.WriteLine($"[SubscriptionRenewalJob] Created {renewedCount} new monthly periods for yearly subscriptions.");
            }
        }
    }
}
