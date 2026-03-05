using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class Location
{
    public int Id { get; set; }

    public string CityName { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
