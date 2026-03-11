using System.ComponentModel.DataAnnotations;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // ========== Admin Edit Profile ==========
    public class AdminEditProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ là bắt buộc.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public string? Avatar { get; set; }
    }

    // ========== Candidate Edit Profile ==========
    public class CandidateEditProfileViewModel
    {
        public int UserId { get; set; }
        public int CandidateId { get; set; }

        // Email is readonly in UI, no validation needed
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ là bắt buộc.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc.")]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? CurrentPosition { get; set; }

        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm không hợp lệ.")]
        public int? YearsOfExperience { get; set; }

        public decimal? CurrentSalary { get; set; }

        public decimal? ExpectedSalary { get; set; }

        public string? Summary { get; set; }

        public bool IsLookingForJob { get; set; }

        public string? Avatar { get; set; }

        // Location fields for UI
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }
    }

    // ========== Recruiter Edit Profile ==========
    public class RecruiterEditProfileViewModel
    {
        public int UserId { get; set; }
        public int RecruiterId { get; set; }
        public int? CompanyId { get; set; }

        // Email is readonly in UI, no validation needed
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ là bắt buộc.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vị trí công việc là bắt buộc.")]
        [MaxLength(255)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        public string? Avatar { get; set; }

        // Company info - now editable
        [Required(ErrorMessage = "Tên công ty là bắt buộc.")]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompanyTaxCode { get; set; }

        [MaxLength(50)]
        public string? CompanySize { get; set; }

        [MaxLength(200)]
        public string? CompanyIndustry { get; set; }

        [MaxLength(500)]
        [Url(ErrorMessage = "URL website không hợp lệ")]
        public string? CompanyWebsite { get; set; }

        [EmailAddress(ErrorMessage = "Email công ty không hợp lệ")]
        [MaxLength(100)]
        public string? CompanyEmail { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? CompanyPhone { get; set; }

        public string? CompanyDescription { get; set; }

        // Location fields
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }
        
        [MaxLength(500)]
        public string? WorkAddress { get; set; }

        public bool IsVerified { get; set; }
    }
}
