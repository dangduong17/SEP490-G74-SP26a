using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
