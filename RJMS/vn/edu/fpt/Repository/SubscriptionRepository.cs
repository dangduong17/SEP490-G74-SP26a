using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using vn.edu.fpt.Utilities;

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
            var plan = new SubscriptionPlan
            {
                Name = model.Name,
                Price = model.Price,
                DurationDays = model.DurationDays,
                BillingCycle = model.BillingCycle,
                Version = model.Version,
                Description = BuildDescription(model),
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

            return plan.Id;
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
            if (combined.Contains("premium")) return "Premium";
            if (combined.Contains("standard")) return "Standard";
            if (combined.Contains("basic")) return "Basic";
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
