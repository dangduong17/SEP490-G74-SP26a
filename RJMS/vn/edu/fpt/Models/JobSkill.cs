using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models;

public partial class JobSkill
{
    public int JobId { get; set; }

    public int SkillId { get; set; }

    public bool? IsRequired { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
