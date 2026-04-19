using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

public partial class SubscriptionPlan
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    [NotMapped]
    public int? DurationDays { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [NotMapped]
    public string? BillingCycle { get; set; }

    [NotMapped]
    public int? Version { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Archive instead of delete for historical data (nullable for database compatibility)
    public bool? IsArchived { get; set; } = false;

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<SubscriptionPlanOption> PlanOptions { get; set; } = new List<SubscriptionPlanOption>();

    public virtual ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();

    public virtual ICollection<SubscriptionPeriod> SubscriptionPeriods { get; set; } = new List<SubscriptionPeriod>();
}
