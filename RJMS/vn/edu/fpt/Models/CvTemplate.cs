using System;

namespace RJMS.vn.edu.fpt.Models;

public class CvTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public string? CssContent { get; set; }
    public string? ConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CategoryId { get; set; }
    public virtual TemplateCategory? Category { get; set; }
}
