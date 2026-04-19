using System.ComponentModel.DataAnnotations;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // ── List page ──────────────────────────────────────────────────────────────
    public class SubscriptionListViewModel
    {
        public List<SubscriptionPlanRowDto> Plans { get; set; } = new();

        // Summary cards
        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int RecruitersUsing { get; set; }

        // Filters
        public string? SearchKeyword { get; set; }
        public string? StatusFilter { get; set; }   // "active" | "inactive" | ""
        public string? TypeFilter { get; set; }      // "Basic" | "Standard" | "Premium" | ""

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class SubscriptionPlanRowDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;       // SUB-001
        public string Name { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;   // Basic | Standard | Premium
        public decimal Price { get; set; }
        public int? JobLimit { get; set; }                     // null = unlimited
        public int? CvAiLimit { get; set; }                    // null = unlimited
        public int DurationDays { get; set; }
        public string? BillingCycle { get; set; }              // Monthly | Yearly | Custom
        public int? Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int RecruiterCount { get; set; }

        // Display helpers
        public string BillingCycleDisplay
        {
            get => BillingCycle == "Yearly" ? "Hàng năm" : "Hàng tháng";
        }

        public string PriceDisplay
        {
            get => $"{Price:N0}₫ / {(BillingCycle == "Yearly" ? "năm" : "tháng")}";
        }

        public decimal? YearlyPrice
        {
            get
            {
                if (BillingCycle == "Yearly") return Price;
                if (BillingCycle == "Monthly") return Price * 12;
                return null;
            }
        }

        public int? DiscountPercentage
        {
            get
            {
                if (BillingCycle != "Yearly") return null;
                // Calculate saving if compared to monthly*12
                var monthlyPrice = YearlyPrice.HasValue ? YearlyPrice.Value / 12 : Price;
                if (monthlyPrice == 0) return 0;
                return (int)((monthlyPrice * 12 - Price) / (monthlyPrice * 12) * 100);
            }
        }
    }

    // ── Create / Edit form ──────────────────────────────────────────────────────
    public class SubscriptionPlanFormViewModel
    {
        public int Id { get; set; }   // 0 = create

        [Required(ErrorMessage = "Tên gói là bắt buộc.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại gói là bắt buộc.")]
        public string PlanType { get; set; } = "Basic";   // Basic | Standard | Premium

        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá năm phải >= 0")]
        public decimal? YearlyPrice { get; set; }

        public bool EnableYearly { get; set; }

        public int? JobLimit { get; set; }      // null = không giới hạn
        public int? CvAiLimit { get; set; }

        [Required(ErrorMessage = "Thời hạn là bắt buộc.")]
        [Range(1, 3650, ErrorMessage = "Thời hạn từ 1–3650 ngày")]
        public int DurationDays { get; set; } = 30;

        /// <summary>
        /// [""] by default. CheckBox: can be ["Monthly"], ["Yearly"], or ["Monthly","Yearly"].
        /// When creating, we insert one plan per selected cycle.
        /// </summary>
        public List<string> BillingCycles { get; set; } = new() { "Monthly" };

        // Used only for single plan edit
        [MaxLength(20)]
        public string? BillingCycle { get; set; } = "Monthly";

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ── Detail / view modal ────────────────────────────────────────────────────
    public class SubscriptionPlanDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? JobLimit { get; set; }
        public int? CvAiLimit { get; set; }
        public int DurationDays { get; set; }
        public string? BillingCycle { get; set; }
        public int? Version { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int RecruiterCount { get; set; }
        public List<ActiveSubscriberDto> RecentSubscribers { get; set; } = new();

        // Display helpers
        public string BillingCycleDisplay
        {
            get => BillingCycle == "Yearly" ? "Hàng năm" : "Hàng tháng";
        }

        public string PriceDisplay
        {
            get => $"{Price:N0}₫ / {(BillingCycle == "Yearly" ? "năm" : "tháng")}";
        }
    }

    public class ActiveSubscriberDto
    {
        public string RecruiterName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ── Quota Check Result (used by Middleware + Lazy Check) ───────────────────
    public class QuotaCheckResult
    {
        public bool Allowed { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
        public int? Limit { get; set; }
        public int Used { get; set; }
        public int Remaining => Limit.HasValue ? Math.Max(0, Limit.Value - Used) : int.MaxValue;
        public string Message { get; set; } = string.Empty;
    }

    // ── Period summary ─────────────────────────────────────────────────────────
    public class SubscriptionPeriodDto
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public bool IsCurrent => DateTime.UtcNow >= PeriodStart && DateTime.UtcNow <= PeriodEnd;
        public List<UsageItemDto> Usages { get; set; } = new();
    }

    public class UsageItemDto
    {
        public string FeatureCode { get; set; } = string.Empty;
        public int UsedCount { get; set; }
        public int? FeatureLimit { get; set; }
    }

    // ── For UI display with grouped plans by billing cycle ──
    public class SubscriptionPlanGroupDto
    {
        public string BaseName { get; set; } = string.Empty;  // Basic | Pro | Enterprise (without cycle suffix)
        public string PlanType { get; set; } = string.Empty;
        public SubscriptionPlanDisplayDto? MonthlyPlan { get; set; }
        public SubscriptionPlanDisplayDto? YearlyPlan { get; set; }

        public bool HasYearly => YearlyPlan != null;
        public bool HasMonthly => MonthlyPlan != null;
    }

    public class SubscriptionPlanDisplayDto
    {
        public int OptionId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? JobLimit { get; set; }
        public int? CvAiLimit { get; set; }
        public int DurationDays { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<PlanFeatureDto> Features { get; set; } = new();

        public string BillingCycleDisplay => BillingCycle == "Yearly" ? "Hàng năm" : "Hàng tháng";

        public string PriceDisplay => $"{Price:N0}₫ / {(BillingCycle == "Yearly" ? "năm" : "tháng")}";
        public int DiscountPercentage { get; set; } = 0;
    }

    public class PlanFeatureDto
    {
        public string FeatureCode { get; set; } = string.Empty;
        public int? FeatureLimit { get; set; }
    }
}

