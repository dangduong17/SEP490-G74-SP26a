using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? CompanyId { get; set; }

    public int PlanId { get; set; }

    public int? PlanOptionId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? AutoRenew { get; set; }

    // Tracking cancellation
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Next auto-billing date (for renewal tracking)
    public DateTime? NextBillingDate { get; set; }

    // Audit trail
    public DateTime? UpdatedAt { get; set; }

    // Snapshot fields to preserve purchase-time plan data
    public decimal? SubscribedPrice { get; set; }
    public string? SubscribedBillingCycle { get; set; }
    public int? SubscribedDurationDays { get; set; }
    public string? SubscribedPlanName { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual SubscriptionPlanOption? PlanOption { get; set; }

    public virtual Company? Company { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<SubscriptionPeriod> SubscriptionPeriods { get; set; } = new List<SubscriptionPeriod>();
}
