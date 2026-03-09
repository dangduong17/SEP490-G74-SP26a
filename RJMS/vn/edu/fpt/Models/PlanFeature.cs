namespace RJMS.vn.edu.fpt.Models;

public partial class PlanFeature
{
    public int Id { get; set; }

    public int PlanId { get; set; }

    public string FeatureCode { get; set; } = null!;

    public int? FeatureLimit { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;
}
