using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

[Table("MessageReads")]
public class MessageRead
{
    [Key]
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual Message Message { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
