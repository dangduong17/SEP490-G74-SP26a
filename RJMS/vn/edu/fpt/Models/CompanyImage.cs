using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class CompanyImage
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
}
