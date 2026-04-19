using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class HomeIndexViewModel
    {
        public int RecruiterCount { get; set; }
        public int CompanyCount { get; set; }
        public int ActiveJobCount { get; set; }
        public int ApplicationCount { get; set; }

        public List<HomeJobCardDTO> LatestJobs { get; set; } = new();
        public List<HomeCompanyCardDTO> TopCompanies { get; set; } = new();
    }

    public class HomeJobCardDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string? LocationName { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? JobType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PublishDate { get; set; }
    }

    public class HomeCompanyCardDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public int ActiveJobCount { get; set; }
    }
}
