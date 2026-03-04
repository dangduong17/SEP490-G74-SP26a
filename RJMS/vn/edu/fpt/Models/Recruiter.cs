using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Recruiter
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int? CompanyId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public string? Department { get; set; }

    public string? Avatar { get; set; }

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? VerificationDocument { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual AspNetUser User { get; set; } = null!;
}
