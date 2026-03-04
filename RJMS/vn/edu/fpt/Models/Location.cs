using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Location
{
    public int Id { get; set; }

    public string CityName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
