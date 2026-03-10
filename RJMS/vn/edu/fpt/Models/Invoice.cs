using System;

namespace RJMS.vn.edu.fpt.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public int SubscriptionId { get; set; }

    public int PaymentId { get; set; }

    public string? InvoiceNumber { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Status { get; set; } // PAID, PENDING, CANCELLED

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;
}
