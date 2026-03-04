using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class FollowedCompany
{
    public int CandidateId { get; set; }

    public int CompanyId { get; set; }

    public DateTime FollowedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;
}
