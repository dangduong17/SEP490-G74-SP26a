using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class RecruiterLocation
{
    public int RecruiterId { get; set; }
    public int CompanyLocationId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime AssignedAt { get; set; }

    public virtual Recruiter Recruiter { get; set; } = null!;
    public virtual CompanyLocation CompanyLocation { get; set; } = null!;
}
