using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class JobCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public string? Slug { get; set; }
        public string? ParentName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ChildrenCount { get; set; }
    }

    public class JobCategoryListViewModel
    {
        public List<JobCategoryViewModel> Categories { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public string? Keyword { get; set; }
        public int? FilterLevel { get; set; }
    }

    public class CreateJobCategoryModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; } = 1;
        public string? Slug { get; set; }
    }

    public class UpdateJobCategoryModel : CreateJobCategoryModel
    {
        public int Id { get; set; }
    }
}
