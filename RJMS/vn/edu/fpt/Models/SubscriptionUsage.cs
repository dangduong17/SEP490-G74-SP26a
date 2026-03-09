namespace RJMS.vn.edu.fpt.Models;

public partial class SubscriptionUsage
{
    public int Id { get; set; }

    public int PeriodId { get; set; }

    public string FeatureCode { get; set; } = null!;

    public int UsedCount { get; set; }

    public virtual SubscriptionPeriod Period { get; set; } = null!;
}
