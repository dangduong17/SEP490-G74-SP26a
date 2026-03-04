using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class JobCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
