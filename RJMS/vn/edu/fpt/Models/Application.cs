using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Application
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int CandidateId { get; set; }

    public int Cvid { get; set; }

    public string? CoverLetter { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Cv Cv { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;
}
