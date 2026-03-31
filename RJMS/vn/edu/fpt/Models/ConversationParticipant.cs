using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

[Table("ConversationParticipants")]
public class ConversationParticipant
{
    [Key]
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int UserId { get; set; }
    public DateTime? JoinedAt { get; set; } = DateTime.Now;

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
