using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

[Table("ConversationJobs")]
public class ConversationJob
{
    [Key]
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int? JobId { get; set; }
    public int? ApplicationId { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual Job? Job { get; set; }
    public virtual Application? Application { get; set; }
}
