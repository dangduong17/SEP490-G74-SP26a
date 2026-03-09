namespace RJMS.vn.edu.fpt.Models;

public partial class SubscriptionPeriod
{
    public int Id { get; set; }

    public int SubscriptionId { get; set; }

    public int PlanId { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual ICollection<SubscriptionUsage> SubscriptionUsages { get; set; } = new List<SubscriptionUsage>();
}
