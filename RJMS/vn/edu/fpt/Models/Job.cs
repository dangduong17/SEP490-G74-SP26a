using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Job
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int CompanyId { get; set; }

    public int RecruiterId { get; set; }

    public int? CompanyAddressId { get; set; }

    public string Description { get; set; } = null!;

    public string? Requirements { get; set; }

    public string? Benefits { get; set; }

    public string? Level { get; set; }

    public string? JobType { get; set; }

    public string? WorkingType { get; set; }

    public decimal? MinSalary { get; set; }

    public decimal? MaxSalary { get; set; }

    public bool IsNegotiable { get; set; }

    public string? SalaryCurrency { get; set; }

    public int NumberOfPositions { get; set; }

    public string? Gender { get; set; }

    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public int? MinYearsOfExperience { get; set; }

    public string? DegreeRequired { get; set; }

    public DateTime? ApplicationDeadline { get; set; }

    public DateTime? StartDate { get; set; }

    public string Status { get; set; } = null!;

    public int ViewCount { get; set; }

    public int ApplicationCount { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsUrgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public int? JobCategoryId { get; set; }

    public int? LocationId { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Company Company { get; set; } = null!;

    public virtual CompanyAddress? CompanyAddress { get; set; }

    public virtual JobCategory? JobCategory { get; set; }

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public virtual Location? Location { get; set; }

    public virtual Recruiter Recruiter { get; set; } = null!;

    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}
