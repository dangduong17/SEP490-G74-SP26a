using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class JobRecruiter
{
    public int JobId { get; set; }
    public int RecruiterId { get; set; }
    public int CompanyLocationId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime AssignedAt { get; set; }

    public virtual Job Job { get; set; } = null!;
    public virtual Recruiter Recruiter { get; set; } = null!;
    public virtual CompanyLocation CompanyLocation { get; set; } = null!;
}
