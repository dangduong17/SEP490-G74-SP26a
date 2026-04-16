using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class CompanyLocation
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int LocationId { get; set; }
    public string? AddressLabel { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual ICollection<RecruiterLocation> RecruiterLocations { get; set; } = new List<RecruiterLocation>();
    public virtual ICollection<JobRecruiter> JobRecruiters { get; set; } = new List<JobRecruiter>();
}
