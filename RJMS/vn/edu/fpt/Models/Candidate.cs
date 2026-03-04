using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Candidate
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public string? Title { get; set; }

    public decimal? CurrentSalary { get; set; }

    public decimal? ExpectedSalary { get; set; }

    public string? WorkingType { get; set; }

    public string? Summary { get; set; }

    public string? CurrentPosition { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? HighestDegree { get; set; }

    public bool IsLookingForJob { get; set; }

    public bool AllowContact { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();

    public virtual ICollection<Cv> Cvs { get; set; } = new List<Cv>();

    public virtual ICollection<Education> Educations { get; set; } = new List<Education>();

    public virtual ICollection<FollowedCompany> FollowedCompanies { get; set; } = new List<FollowedCompany>();

    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
}
