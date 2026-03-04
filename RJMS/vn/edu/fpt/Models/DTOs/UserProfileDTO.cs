using System;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class UserProfileDTO
    {
        public int CandidateId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal? CurrentSalary { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public string WorkingType { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string CurrentPosition { get; set; } = string.Empty;
        public int? YearsOfExperience { get; set; }
        public string HighestDegree { get; set; } = string.Empty;
        public bool IsLookingForJob { get; set; }
        public bool AllowContact { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
