using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public class TemplateCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<CvTemplate> CvTemplates { get; set; } = new List<CvTemplate>();
}
