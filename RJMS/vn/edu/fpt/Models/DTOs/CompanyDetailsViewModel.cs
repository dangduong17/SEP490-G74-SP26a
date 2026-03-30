using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class CompanyDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Logo { get; set; }
        public string? CoverImage { get; set; }
        public string? Description { get; set; }
        public string? Benefits { get; set; }
        public string? CompanySize { get; set; }
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public bool? IsVerified { get; set; }
        public int FollowerCount { get; set; }
        public bool IsFollowing { get; set; }
        
        public List<PublicJobListItemDTO> ActiveJobs { get; set; } = new();
    }
}
