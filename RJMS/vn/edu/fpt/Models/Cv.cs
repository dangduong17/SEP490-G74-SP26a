using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Cv
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public string? Title { get; set; }

    public string? FilePath { get; set; }

    public int? ViewCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Candidate Candidate { get; set; } = null!;
}
