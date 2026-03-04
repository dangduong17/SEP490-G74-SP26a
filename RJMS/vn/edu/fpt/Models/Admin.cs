using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Admin
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public string? Department { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
