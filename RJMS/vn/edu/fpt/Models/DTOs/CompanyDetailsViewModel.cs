using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class CompanyListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Logo { get; set; }
        public string? CoverImage { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? CityName { get; set; }
        public int ActiveJobCount { get; set; }
        public int FollowerCount { get; set; }
    }

    public class CompanyListViewModel
    {
        public string? Keyword { get; set; }
        public string? Industry { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public List<CompanyListItemViewModel> Companies { get; set; } = new();
    }

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
