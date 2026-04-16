using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using vn.edu.fpt.Utilities;
using System.Text;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly FindingJobsDbContext _context;

        public SubscriptionRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionListViewModel> GetPlanListAsync(
            string? keyword, string? status, string? type, int page, int pageSize)
        {
            var query = _context.SubscriptionPlans.AsQueryable();

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.Name != null && p.Name.Contains(keyword));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.ToLower() == "active";
                query = query.Where(p => p.IsActive == isActive);
            }

            // Filter by type (based on Name or Description)
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => 
                    (p.Name != null && p.Name.Contains(type)) ||
                    (p.Description != null && p.Description.Contains(type)));
            }

            var totalItems = await query.CountAsync();

            var plans = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Subscriptions)
                .Include(p => p.PlanFeatures)
                .ToListAsync();

            var planDtos = plans.Select(p => new SubscriptionPlanRowDto
            {
                Id = p.Id,
                Code = $"SUB-{p.Id:D3}",
                Name = p.Name ?? "",
                PlanType = ExtractPlanType(p.Name, p.Description),
                Price = p.Price ?? 0,
                JobLimit = p.PlanFeatures.FirstOrDefault(f => f.FeatureCode == "JOB_POSTING")?.FeatureLimit,
                CvAiLimit = p.PlanFeatures.FirstOrDefault(f => f.FeatureCode == "CV_AI_FILTER")?.FeatureLimit,
                DurationDays = p.DurationDays ?? 30,
                BillingCycle = p.BillingCycle,
                Version = p.Version,
                IsActive = p.IsActive ?? false,
                CreatedAt = p.CreatedAt,
                RecruiterCount = p.Subscriptions.Count(s => s.Status == "Active")
            }).ToList();

            return new SubscriptionListViewModel
            {
                Plans = planDtos,
                TotalPlans = await _context.SubscriptionPlans.CountAsync(),
                ActivePlans = await _context.SubscriptionPlans.CountAsync(p => p.IsActive == true),
                RecruitersUsing = await _context.Subscriptions
                    .Where(s => s.Status == "Active")
                    .Select(s => s.UserId)
                    .Distinct()
                    .CountAsync(),
                SearchKeyword = keyword,
                StatusFilter = status,
                TypeFilter = type,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<SubscriptionPlanDetailDto?> GetPlanDetailAsync(int id)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.Subscriptions)
                    .ThenInclude(s => s.User)
                        .ThenInclude(u => u.Recruiters)
                .Include(p => p.PlanFeatures)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return null;

            var activeSubscribers = plan.Subscriptions
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .Take(5)
                .Select(s => new ActiveSubscriberDto
                {
                    RecruiterName = $"{s.User.FirstName} {s.User.LastName}",
                    CompanyName = s.User.Recruiters.FirstOrDefault()?.Company?.Name ?? "N/A",
                    EndDate = s.EndDate,
                    Status = s.Status ?? "Unknown"
                })
                .ToList();

            return new SubscriptionPlanDetailDto
            {
                Id = plan.Id,
                Name = plan.Name ?? "",
                PlanType = ExtractPlanType(plan.Name, plan.Description),
                Price = plan.Price ?? 0,
                JobLimit = plan.PlanFeatures.FirstOrDefault(f => f.FeatureCode == "JOB_POSTING")?.FeatureLimit,
                CvAiLimit = plan.PlanFeatures.FirstOrDefault(f => f.FeatureCode == "CV_AI_FILTER")?.FeatureLimit,
                DurationDays = plan.DurationDays ?? 30,
                BillingCycle = plan.BillingCycle,
                Version = plan.Version,
                Description = plan.Description,
                IsActive = plan.IsActive ?? false,
                CreatedAt = plan.CreatedAt,
                RecruiterCount = plan.Subscriptions.Count(s => s.Status == "Active"),
                RecentSubscribers = activeSubscribers
            };
        }

        public async Task<SubscriptionPlanFormViewModel?> GetPlanForEditAsync(int id)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.PlanFeatures)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return null;

            return new SubscriptionPlanFormViewModel
            {
                Id = plan.Id,
                Name = plan.Name ?? "",
                PlanType = ExtractPlanType(plan.Name, plan.Description),
                Price = plan.Price ?? 0,
                JobLimit = plan.PlanFeatures.FirstOrDefault(f => f.FeatureCode == "JOB_POSTING")?.FeatureLimit,
                CvAiLimit = plan.PlanFeatures.FirstOrDefault(f => f.FeatureCode == "CV_AI_FILTER")?.FeatureLimit,
                DurationDays = plan.DurationDays ?? 30,
                BillingCycle = plan.BillingCycle ?? "Monthly",
                Version = plan.Version ?? 1,
                Description = plan.Description,
                IsActive = plan.IsActive ?? false
            };
        }

        public async Task<int> CreatePlanAsync(SubscriptionPlanFormViewModel model)
        {
            var plan = await CreateOnePlanAsync(model, model.BillingCycle ?? "Monthly");
            return plan.Id;
        }

        public async Task<List<int>> CreatePlansForCyclesAsync(SubscriptionPlanFormViewModel model)
        {
            var cycles = model.BillingCycles
                ?? new List<string> { model.BillingCycle ?? "Monthly" };
            if (!cycles.Any()) cycles = new List<string> { "Monthly" };

            var ids = new List<int>();
            foreach (var cycle in cycles.Distinct())
            {
                var plan = await CreateOnePlanAsync(model, cycle);
                ids.Add(plan.Id);
            }
            return ids;
        }

        private async Task<SubscriptionPlan> CreateOnePlanAsync(SubscriptionPlanFormViewModel model, string cycle)
        {
            var planName = model.Name;
            var duration = model.DurationDays;

            if (cycle == "Yearly")
            {
                planName += " (Hàng năm)";
                if (duration <= 31) duration = 365; // Auto-adjust if it looks like a monthly duration
            }
            else if (cycle == "Monthly")
            {
                planName += " (Hàng tháng)";
                if (duration == 0 || duration > 365) duration = 30; 
            }

            var plan = new SubscriptionPlan
            {
                Name = planName,
                Price = model.Price,
                DurationDays = duration,
                BillingCycle = cycle,
                Version = model.Version,
                Description = model.Description ?? BuildDescription(model),
                IsActive = model.IsActive,
                CreatedAt = DateTimeHelper.NowVietnam
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            // Create PlanFeature records
            if (model.JobLimit.HasValue)
            {
                _context.PlanFeatures.Add(new PlanFeature
                {
                    PlanId = plan.Id,
                    FeatureCode = "JOB_POSTING",
                    FeatureLimit = model.JobLimit.Value
                });
            }

            if (model.CvAiLimit.HasValue)
            {
                _context.PlanFeatures.Add(new PlanFeature
                {
                    PlanId = plan.Id,
                    FeatureCode = "CV_AI_FILTER",
                    FeatureLimit = model.CvAiLimit.Value
                });
            }

            await _context.SaveChangesAsync();

            return plan;
        }

        public async Task<bool> UpdatePlanAsync(SubscriptionPlanFormViewModel model)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.PlanFeatures)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (plan == null) return false;

            plan.Name = model.Name;
            plan.Price = model.Price;
            plan.DurationDays = model.DurationDays;
            plan.BillingCycle = model.BillingCycle;
            plan.Version = model.Version;
            plan.Description = BuildDescription(model);
            plan.IsActive = model.IsActive;

            // Remove existing PlanFeatures
            _context.PlanFeatures.RemoveRange(plan.PlanFeatures);

            // Add new PlanFeatures
            if (model.JobLimit.HasValue)
            {
                _context.PlanFeatures.Add(new PlanFeature
                {
                    PlanId = plan.Id,
                    FeatureCode = "JOB_POSTING",
                    FeatureLimit = model.JobLimit.Value
                });
            }

            if (model.CvAiLimit.HasValue)
            {
                _context.PlanFeatures.Add(new PlanFeature
                {
                    PlanId = plan.Id,
                    FeatureCode = "CV_AI_FILTER",
                    FeatureLimit = model.CvAiLimit.Value
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ───────────────────────────────────────────────────────────────
        // Period / Renewal
        // ───────────────────────────────────────────────────────────────
        public async Task<SubscriptionPeriodDto?> GetCurrentPeriodAsync(int subscriptionId)
        {
            var now = DateTime.UtcNow;
            var period = await _context.SubscriptionPeriods
                .Include(p => p.Plan)
                    .ThenInclude(pl => pl.PlanFeatures)
                .Include(p => p.SubscriptionUsages)
                .Where(p => p.SubscriptionId == subscriptionId && p.PeriodStart <= now && p.PeriodEnd >= now)
                .OrderByDescending(p => p.PeriodStart)
                .FirstOrDefaultAsync();

            if (period == null) return null;

            return new SubscriptionPeriodDto
            {
                Id = period.Id,
                SubscriptionId = period.SubscriptionId,
                PlanId = period.PlanId,
                PlanName = period.Plan.Name ?? "",
                PeriodStart = period.PeriodStart,
                PeriodEnd = period.PeriodEnd,
                Usages = period.SubscriptionUsages.Select(u => new UsageItemDto
                {
                    FeatureCode = u.FeatureCode,
                    UsedCount = u.UsedCount,
                    FeatureLimit = period.Plan.PlanFeatures
                        .FirstOrDefault(f => f.FeatureCode == u.FeatureCode)?.FeatureLimit
                }).ToList()
            };
        }

        public async Task CreatePeriodAsync(int subscriptionId, int planId, DateTime start, DateTime end)
        {
            _context.SubscriptionPeriods.Add(new SubscriptionPeriod
            {
                SubscriptionId = subscriptionId,
                PlanId = planId,
                PeriodStart = start,
                PeriodEnd = end
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Call from Hangfire Job daily: for every Active, Yearly subscription where the current
        /// month has no SubscriptionPeriod, create one covering the 1st to last day of that month.
        /// Returns count of periods created.
        /// </summary>
        public async Task<int> RenewExpiredPeriodsAsync()
        {
            var now = DateTime.UtcNow;
            var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddSeconds(-1);

            // Active yearly subscriptions still within their EndDate
            var yearlyActive = await _context.Subscriptions
                .Where(s => s.Status == "Active" && s.EndDate >= now)
                .Where(s => s.Plan.BillingCycle == "Yearly")
                .Include(s => s.Plan)
                .Include(s => s.SubscriptionPeriods)
                .ToListAsync();

            int created = 0;
            foreach (var sub in yearlyActive)
            {
                bool hasCurrent = sub.SubscriptionPeriods
                    .Any(p => p.PeriodStart <= now && p.PeriodEnd >= now);

                if (!hasCurrent)
                {
                    _context.SubscriptionPeriods.Add(new SubscriptionPeriod
                    {
                        SubscriptionId = sub.Id,
                        PlanId = sub.PlanId,
                        PeriodStart = firstOfMonth,
                        PeriodEnd = lastOfMonth
                    });
                    created++;
                }
            }

            if (created > 0) await _context.SaveChangesAsync();
            return created;
        }

        public async Task<int> ProcessExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Subscriptions
                .Where(s => s.Status == "Active" && (s.EndDate < now || s.StartDate > now))
                .ToListAsync();

            foreach (var sub in expired)
            {
                sub.Status = "Expired";
            }

            if (expired.Any()) await _context.SaveChangesAsync();
            return expired.Count;
        }

        // ───────────────────────────────────────────────────────────────
        // Quota
        // ───────────────────────────────────────────────────────────────
        public async Task<QuotaCheckResult> CheckQuotaAsync(int userId, string featureCode)
        {
            var now = DateTime.UtcNow;

            var sub = await _context.Subscriptions
                .Include(s => s.Plan).ThenInclude(p => p.PlanFeatures)
                .Include(s => s.SubscriptionPeriods).ThenInclude(p => p.SubscriptionUsages)
                .Where(s => s.UserId == userId && (s.Status == "Active" || s.Status == "ACTIVE") && s.EndDate >= now)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (sub == null)
            {
                sub = await CreateDefaultFreeSubscriptionAsync(userId, now);
            }

            if (sub == null)
                return new QuotaCheckResult { Allowed = false, FeatureCode = featureCode, Message = "Không tìm thấy gói dịch vụ khả dụng." };

            // Lazy create period if missing for Yearly subs
            var period = sub.SubscriptionPeriods
                .FirstOrDefault(p => p.PeriodStart <= now && p.PeriodEnd >= now);

            if (period == null && sub.Plan.BillingCycle == "Yearly")
            {
                var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var lastOfMonth = firstOfMonth.AddMonths(1).AddSeconds(-1);
                var newPeriod = new SubscriptionPeriod
                {
                    SubscriptionId = sub.Id,
                    PlanId = sub.PlanId,
                    PeriodStart = firstOfMonth,
                    PeriodEnd = lastOfMonth
                };
                _context.SubscriptionPeriods.Add(newPeriod);
                await _context.SaveChangesAsync();
                period = newPeriod;
            }

            if (period == null)
                return new QuotaCheckResult { Allowed = false, FeatureCode = featureCode, Message = "Không tìm thấy kỳ dịch vụ hiện tại." };

            var feature = sub.Plan.PlanFeatures.FirstOrDefault(f => f.FeatureCode == featureCode);
            var limit = feature?.FeatureLimit;

            var usage = period.SubscriptionUsages
                .FirstOrDefault(u => u.FeatureCode == featureCode);
            int used = usage?.UsedCount ?? 0;

            if (limit.HasValue && used >= limit.Value)
                return new QuotaCheckResult
                {
                    Allowed = false, FeatureCode = featureCode,
                    Limit = limit, Used = used,
                    Message = $"Bạn đã sử dụng hết {limit} lượt {featureCode} trong kỳ này."
                };

            return new QuotaCheckResult
            {
                Allowed = true, FeatureCode = featureCode,
                Limit = limit, Used = used, Message = "OK"
            };
        }

        public async Task ConsumeQuotaAsync(int userId, string featureCode)
        {
            var now = DateTime.UtcNow;

            var sub = await _context.Subscriptions
                .Include(s => s.SubscriptionPeriods).ThenInclude(p => p.SubscriptionUsages)
                .Where(s => s.UserId == userId && (s.Status == "Active" || s.Status == "ACTIVE") && s.EndDate >= now)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (sub == null)
            {
                sub = await CreateDefaultFreeSubscriptionAsync(userId, now);
                if (sub == null) return;
            }

            var period = sub.SubscriptionPeriods
                .FirstOrDefault(p => p.PeriodStart <= now && p.PeriodEnd >= now);
            if (period == null) return;

            var usage = period.SubscriptionUsages
                .FirstOrDefault(u => u.FeatureCode == featureCode);

            if (usage == null)
            {
                _context.SubscriptionUsages.Add(new SubscriptionUsage
                {
                    PeriodId = period.Id,
                    FeatureCode = featureCode,
                    UsedCount = 1
                });
            }
            else
            {
                usage.UsedCount++;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Subscription?> CreateDefaultFreeSubscriptionAsync(int userId, DateTime now)
        {
            var freePlan = await _context.SubscriptionPlans
                .Include(sp => sp.PlanFeatures)
                .Where(sp => sp.IsActive == true && ((sp.Price ?? 0) == 0 || sp.Name == "Gói Miễn Phí"))
                .OrderBy(sp => sp.Id)
                .FirstOrDefaultAsync();

            if (freePlan == null)
            {
                return null;
            }

            var companyId = await _context.Recruiters
                .Where(r => r.UserId == userId)
                .Select(r => r.CompanyId)
                .FirstOrDefaultAsync();

            var startDate = now;
            var endDate = now.AddDays(freePlan.DurationDays ?? 30);

            var subscription = new Subscription
            {
                UserId = userId,
                CompanyId = companyId,
                PlanId = freePlan.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Active",
                CreatedAt = now,
                AutoRenew = false
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var period = new SubscriptionPeriod
            {
                SubscriptionId = subscription.Id,
                PlanId = freePlan.Id,
                PeriodStart = startDate,
                PeriodEnd = endDate
            };

            _context.SubscriptionPeriods.Add(period);
            await _context.SaveChangesAsync();

            foreach (var feature in freePlan.PlanFeatures)
            {
                _context.SubscriptionUsages.Add(new SubscriptionUsage
                {
                    PeriodId = period.Id,
                    FeatureCode = feature.FeatureCode,
                    UsedCount = 0
                });
            }

            if (freePlan.PlanFeatures.Any())
            {
                await _context.SaveChangesAsync();
            }

            return await _context.Subscriptions
                .Include(s => s.Plan).ThenInclude(p => p.PlanFeatures)
                .Include(s => s.SubscriptionPeriods).ThenInclude(p => p.SubscriptionUsages)
                .FirstOrDefaultAsync(s => s.Id == subscription.Id);
        }

        public async Task<bool> TogglePlanStatusAsync(int id)
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return false;

            plan.IsActive = !(plan.IsActive ?? false);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlanAsync(int id)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return false;

            // Check if plan has active subscriptions
            if (plan.Subscriptions.Any(s => s.Status == "Active"))
            {
                return false; // Cannot delete plan with active subscriptions
            }

            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        // Helper methods
        private string ExtractPlanType(string? name, string? description)
        {
            var combined = $"{name} {description}".ToLower();
            if (combined.Contains("#level:premium") || combined.Contains(" premium")) return "Premium";
            if (combined.Contains("#level:standard") || combined.Contains(" standard")) return "Standard";
            if (combined.Contains("#level:basic") || combined.Contains(" basic")) return "Basic";
            return "Basic";
        }

        private int? ExtractFeatureLimit(string? description, string featureType)
        {
            if (string.IsNullOrEmpty(description)) return null;

            var lines = description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.ToLower().Contains(featureType))
                {
                    if (line.ToLower().Contains("unlimited") || line.ToLower().Contains("không giới hạn"))
                        return null;

                    var numbers = System.Text.RegularExpressions.Regex.Matches(line, @"\d+");
                    if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out int limit))
                        return limit;
                }
            }
            return null;
        }

        private string BuildDescription(SubscriptionPlanFormViewModel model)
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(model.PlanType))
                parts.Add($"#LEVEL:{model.PlanType}");

            if (!string.IsNullOrEmpty(model.Description))
                parts.Add(model.Description);

            if (model.JobLimit.HasValue)
                parts.Add($"Job postings: {model.JobLimit} per month");
            else
                parts.Add("Job postings: Unlimited");

            if (model.CvAiLimit.HasValue)
                parts.Add($"CV AI analysis: {model.CvAiLimit} per month");
            else
                parts.Add("CV AI analysis: Unlimited");

            return string.Join("\n", parts);
        }
    }
}
