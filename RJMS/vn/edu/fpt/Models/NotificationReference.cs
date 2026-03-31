using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

[Table("NotificationReferences")]
public class NotificationReference
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    
    // MESSAGE, JOB, APPLICATION, PAYMENT
    public string? ReferenceType { get; set; }
    
    public int? ReferenceId { get; set; }

    public virtual AppNotification Notification { get; set; } = null!;
}
