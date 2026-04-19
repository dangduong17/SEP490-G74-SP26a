using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class SubscriptionPlanOption
{
    public int Id { get; set; }

    public int PlanId { get; set; }

    public string BillingCycle { get; set; } = "Monthly";

    public decimal? Price { get; set; }

    public int? DurationDays { get; set; }

    public bool? IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
