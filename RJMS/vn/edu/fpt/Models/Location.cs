using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Location
{
    public int Id { get; set; }

    public string CityName { get; set; } = null!;

    public int? ProvinceCode { get; set; }

    public int? WardCode { get; set; }

    public string? WardName { get; set; }

    public string? Address { get; set; }

    public string? DetailAddress { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
