using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RJMS.vn.edu.fpt.Models
{
    public class CompanyFollower
    {
        [Key, Column(Order = 0)]
        public int CompanyId { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
