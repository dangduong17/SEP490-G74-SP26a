using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Logo { get; set; }

    public string? CoverImage { get; set; }

    public string? TaxCode { get; set; }

    public string? CompanySize { get; set; }

    public string? Industry { get; set; }

    public string? Website { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Description { get; set; }

    public string? Benefits { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<Recruiter> Recruiters { get; set; } = new List<Recruiter>();
    public virtual ICollection<CompanyFollower> Followers { get; set; } = new List<CompanyFollower>();
    public virtual ICollection<CompanyLocation> CompanyLocations { get; set; } = new List<CompanyLocation>();
}
