using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models;

[Table("MessageAttachments")]
public class MessageAttachment
{
    [Key]
    public int Id { get; set; }
    public int MessageId { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }

    public virtual Message Message { get; set; } = null!;
}
