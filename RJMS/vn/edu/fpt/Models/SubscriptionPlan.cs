using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class SubscriptionPlan
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public int? DurationDays { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? BillingCycle { get; set; }

    public int? Version { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();

    public virtual ICollection<SubscriptionPeriod> SubscriptionPeriods { get; set; } = new List<SubscriptionPeriod>();
}
