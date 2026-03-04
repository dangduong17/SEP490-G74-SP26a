using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Cv
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public string Title { get; set; } = null!;

    public string? FilePath { get; set; }

    public string? TemplateId { get; set; }

    public string? JsonData { get; set; }

    public bool IsDefault { get; set; }

    public int ViewCount { get; set; }

    public int DownloadCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Candidate Candidate { get; set; } = null!;
}
