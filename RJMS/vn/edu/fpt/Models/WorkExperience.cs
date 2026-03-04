using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class WorkExperience
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrentlyWorking { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;
}
