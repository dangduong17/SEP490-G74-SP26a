using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Recruiter
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? CompanyId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public string? Avatar { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual User User { get; set; } = null!;
}
