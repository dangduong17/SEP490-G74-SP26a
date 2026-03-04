using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class SubscriptionPlan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int TargetAudience { get; set; }

    public virtual ICollection<Subscription> SubscriptionPlans { get; set; } = new List<Subscription>();

    public virtual ICollection<Subscription> SubscriptionSubscriptionPlans { get; set; } = new List<Subscription>();
}
