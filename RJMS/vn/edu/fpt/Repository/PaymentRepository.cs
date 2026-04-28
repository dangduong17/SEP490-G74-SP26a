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
            var options = await _context.SubscriptionPlanOptions
                .Include(o => o.Plan)
                    .ThenInclude(p => p.PlanFeatures)
                .Where(o => o.IsActive == true && o.Plan.IsActive == true)
                .OrderBy(o => o.Price)
                .ToListAsync();

            return options.Select(o => new SubscriptionPlan
            {
                Id = o.Id,
                Name = $"{o.Plan.Name} ({(o.BillingCycle == "Yearly" ? "Hàng năm" : "Hàng tháng")})",
                Price = o.Price,
                DurationDays = o.DurationDays,
                Description = o.Plan.Description,
                IsActive = (o.IsActive ?? false) && (o.Plan.IsActive ?? false),
                BillingCycle = o.BillingCycle,
                CreatedAt = o.CreatedAt ?? o.Plan.CreatedAt,
                PlanFeatures = o.Plan.PlanFeatures
            }).ToList();
        }

        public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int planId)
        {
            var option = await _context.SubscriptionPlanOptions
                .Include(o => o.Plan)
                    .ThenInclude(p => p.PlanFeatures)
                .FirstOrDefaultAsync(o => o.Id == planId && o.IsActive == true && o.Plan.IsActive == true);

            if (option == null) return null;

            return new SubscriptionPlan
            {
                Id = option.Id,
                Name = $"{option.Plan.Name} ({(option.BillingCycle == "Yearly" ? "Hàng năm" : "Hàng tháng")})",
                Price = option.Price,
                DurationDays = option.DurationDays,
                Description = option.Plan.Description,
                IsActive = (option.IsActive ?? false) && (option.Plan.IsActive ?? false),
                BillingCycle = option.BillingCycle,
                CreatedAt = option.CreatedAt ?? option.Plan.CreatedAt,
                PlanFeatures = option.Plan.PlanFeatures
            };
        }

        public async Task<int> CreateSubscriptionAsync(int userId, int planId)
        {
            var option = await _context.SubscriptionPlanOptions
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o => o.Id == planId && o.IsActive == true && o.Plan.IsActive == true);
            if (option == null) return 0;

            var companyId = await _context.Recruiters
                .Where(r => r.UserId == userId && r.CompanyId != null)
                .Select(r => r.CompanyId)
                .FirstOrDefaultAsync();

            var subscription = new Subscription
            {
                UserId = userId,
                CompanyId = companyId,
                PlanId = option.PlanId,
                PlanOptionId = option.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(option.DurationDays ?? 30),
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                AutoRenew = false,
                SubscribedPrice = option.Price,
                SubscribedBillingCycle = option.BillingCycle,
                SubscribedDurationDays = option.DurationDays ?? 30,
                SubscribedPlanName = option.Plan.Name
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
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.PlanOption)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.PlanOption)
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
                .Include(s => s.PlanOption)
                .Where(s => s.UserId == userId && s.Status == "ACTIVE" && s.StartDate <= now && s.EndDate >= now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task CancelPreviousActiveSubscriptionsAsync(int userId, int? companyId, int newSubscriptionId)
        {
            var now = DateTime.UtcNow;

            // Tìm tất cả subscription đang Active của user/công ty (trừ subscription mới)
            var query = _context.Subscriptions
                .Where(s => s.Id != newSubscriptionId
                    && (s.Status == "Active" || s.Status == "ACTIVE")
                    && s.EndDate >= now);

            if (companyId.HasValue)
            {
                query = query.Where(s => s.CompanyId == companyId.Value || s.UserId == userId);
            }
            else
            {
                query = query.Where(s => s.UserId == userId);
            }

            var oldSubs = await query.ToListAsync();
            foreach (var sub in oldSubs)
            {
                sub.Status = "Expired";
                sub.CancelledAt = now;
                sub.AutoRenew = false;
                sub.EndDate = now;  // Kết thúc ngay khi upgrade
                sub.UpdatedAt = now;
            }

            if (oldSubs.Any())
                await _context.SaveChangesAsync();
        }
    }
}
