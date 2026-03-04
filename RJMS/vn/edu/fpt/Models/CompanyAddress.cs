using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class CompanyAddress
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Address { get; set; } = null!;

    public string? City { get; set; }

    public string? District { get; set; }

    public string? Ward { get; set; }

    public string? AddressType { get; set; }

    public bool IsHeadquarter { get; set; }

    public string? Phone { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
