using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public bool? IsActive { get; set; }

    public bool? EmailConfirmed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public virtual ICollection<Recruiter> Recruiters { get; set; } = new List<Recruiter>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
