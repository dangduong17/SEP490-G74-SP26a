using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

[Table("Messages")]
public class Message
{
    [Key]
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string? Content { get; set; }
    public string MessageType { get; set; } = "TEXT";
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public bool? IsDeleted { get; set; } = false;

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    public virtual ICollection<MessageRead> Reads { get; set; } = new List<MessageRead>();
}
