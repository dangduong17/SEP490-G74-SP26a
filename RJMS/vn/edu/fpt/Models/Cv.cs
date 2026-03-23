using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Cv
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string? Title { get; set; }

    /// <summary>UPLOAD | BUILDER</summary>
    public string CvType { get; set; } = "UPLOAD";

    // Upload fields
    public string? LegacyFilePath { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public int? FileSize { get; set; }

    // Builder fields
    public int? TemplateId { get; set; }

    public bool IsDefault { get; set; } = false;
    public int? ViewCount { get; set; } = 0;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Nav props
    public virtual Candidate Candidate { get; set; } = null!;
    public virtual CvTemplate? Template { get; set; }
    public virtual CvData? CvData { get; set; }
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
