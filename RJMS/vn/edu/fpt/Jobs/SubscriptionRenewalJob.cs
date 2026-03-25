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
            await _subscriptionService.RenewExpiredPeriodsAsync();
        }
    }
}
