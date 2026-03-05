using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Candidate
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public string? Title { get; set; }

    public decimal? CurrentSalary { get; set; }

    public decimal? ExpectedSalary { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? Summary { get; set; }

    public bool? IsLookingForJob { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Cv> Cvs { get; set; } = new List<Cv>();

    public virtual User User { get; set; } = null!;
}
