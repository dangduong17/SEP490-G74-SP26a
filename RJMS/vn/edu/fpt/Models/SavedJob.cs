using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class SavedJob
{
    public int CandidateId { get; set; }

    public int JobId { get; set; }

    public DateTime SavedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;
}
