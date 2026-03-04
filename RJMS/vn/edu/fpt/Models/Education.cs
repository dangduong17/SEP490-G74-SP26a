using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Education
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public string School { get; set; } = null!;

    public string? Major { get; set; }

    public string? Degree { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrentlyStudying { get; set; }

    public string? Description { get; set; }

    public decimal? Gpa { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;
}
