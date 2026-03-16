using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class JobCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public int Level { get; set; } = 1;

    public string? Slug { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual JobCategory? Parent { get; set; }

    public virtual ICollection<JobCategory> Children { get; set; } = new List<JobCategory>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
