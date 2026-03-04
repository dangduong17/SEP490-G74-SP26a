using System;
using System.Collections.Generic;

namespace RJMS.Models;

public partial class CandidateSkill
{
    public int CandidateId { get; set; }

    public int SkillId { get; set; }

    public string? Level { get; set; }

    public int? YearsOfExperience { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
