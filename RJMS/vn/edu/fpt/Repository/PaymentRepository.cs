using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly FindingJobsDbContext _context;

        public PaymentRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Include(p => p.PlanFeatures)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int planId)
        {
            return await _context.SubscriptionPlans
                .Include(p => p.PlanFeatures)
                .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive == true);
        }

        public async Task<int> CreateSubscriptionAsync(int userId, int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return 0;

            var subscription = new Subscription
            {
                UserId = userId,
                PlanId = planId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationDays ?? 30),
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                AutoRenew = false
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription.Id;
        }

        public async Task<int> CreatePaymentAsync(int subscriptionId, decimal amount)
        {
            var payment = new Payment
            {
                SubscriptionId = subscriptionId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                Status = "PENDING",
                PaymentMethod = "VNPay"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment.Id;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status, string transactionId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            payment.Status = status;
            payment.TransactionId = transactionId;
            payment.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSubscriptionStatusAsync(int subscriptionId, string status)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null) return false;

            subscription.Status = status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateSubscriptionPeriodAsync(int subscriptionId, int planId, DateTime startDate, DateTime endDate)
        {
            var period = new SubscriptionPeriod
            {
                SubscriptionId = subscriptionId,
                PlanId = planId,
                PeriodStart = startDate,
                PeriodEnd = endDate
            };

            _context.SubscriptionPeriods.Add(period);
            await _context.SaveChangesAsync();
            
            var planFeatures = await _context.PlanFeatures.Where(pf => pf.PlanId == planId).ToListAsync();
            foreach (var feature in planFeatures)
            {
                var usage = new SubscriptionUsage
                {
                    PeriodId = period.Id,
                    FeatureCode = feature.FeatureCode,
                    UsedCount = 0
                };
                _context.SubscriptionUsages.Add(usage);
            }
            if (planFeatures.Any())
            {
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> CreateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice.Id;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && s.Status == "ACTIVE" && s.StartDate <= now && s.EndDate >= now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }
    }
}
