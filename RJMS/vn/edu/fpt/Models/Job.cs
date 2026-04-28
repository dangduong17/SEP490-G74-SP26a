using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int CompanyId { get; set; }
    public int? JobCategoryId { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public string? JobType { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public int? NumberOfPositions { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? PublishDate { get; set; }
    public string? Status { get; set; }
    public int? ViewCount { get; set; }
    public int? ApplicationCount { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Admin ban
    public bool IsBanned { get; set; } = false;
    public DateTime? BannedAt { get; set; }
    public string? BanReason { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual Company Company { get; set; } = null!;
    public virtual JobCategory? JobCategory { get; set; }
    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    // Location and Recruiter now via JobRecruiters junction
    public virtual ICollection<JobRecruiter> JobRecruiters { get; set; } = new List<JobRecruiter>();
    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}
