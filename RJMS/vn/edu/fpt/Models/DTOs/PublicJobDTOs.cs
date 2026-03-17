using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class PublicJobListItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string? LocationName { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? JobType { get; set; }
        public string? CategoryName { get; set; }
    }

    public class JobFilterCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int JobCount { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; } = 1;
        public List<JobFilterCategoryDTO> Children { get; set; } = new();
    }

    public class JobFilterLocationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int JobCount { get; set; }
    }

    public class PublicJobListViewModel
    {
        public List<PublicJobListItemDTO> Jobs { get; set; } = new();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // Filters
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }

        // Filter options from DB
        public List<JobFilterCategoryDTO> Categories { get; set; } = new();
        /// <summary>Top-level parent groups, each with their Children filled.</summary>
        public List<JobFilterCategoryDTO> CategoryGroups { get; set; } = new();
        public List<JobFilterLocationDTO> Locations { get; set; } = new();
    }

    // Job Detail ViewModel
    public class JobDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }
        public string? JobType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int? NumberOfPositions { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }

        // Location - full detail
        public string? LocationName { get; set; }      // CityName
        public string? LocationWardName { get; set; }   // WardName
        public string? LocationProvinceName { get; set; } // from Company or Location
        public string? LocationAddress { get; set; }    // street address

        // Category
        public string? CategoryName { get; set; }

        // Skills
        public List<string> Skills { get; set; } = new();

        // Company info
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyProvince { get; set; }
    }
}
