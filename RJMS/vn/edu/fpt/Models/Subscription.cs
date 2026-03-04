using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int PlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? SubscriptionPlanId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
