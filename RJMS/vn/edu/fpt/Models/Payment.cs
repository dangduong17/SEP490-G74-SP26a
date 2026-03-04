using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? TransactionId { get; set; }

    public string Status { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;
}
