using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

public class AppNotification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    // CHAT, APPLICATION, SYSTEM, PAYMENT, JOB
    public string? Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<NotificationReference> References { get; set; } = new List<NotificationReference>();
}
