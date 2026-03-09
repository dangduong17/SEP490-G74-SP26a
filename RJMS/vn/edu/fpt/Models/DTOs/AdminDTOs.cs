using System.ComponentModel.DataAnnotations;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // ---------- Location Lookup ----------

    public class ProvinceLookupDto
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class WardLookupDto
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // ---------- ServiceResult ----------

    public class ServiceError
    {
        public string? Key { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public List<ServiceError> Errors { get; set; } = new();

        public static ServiceResult Success() => new() { Succeeded = true };

        public static ServiceResult NotFoundResult() => new() { Succeeded = false, NotFound = true };

        public static ServiceResult Failed(params ServiceError[] errors) =>
            new() { Succeeded = false, Errors = errors.ToList() };
    }

    // ---------- Dashboard ----------

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalRecruiters { get; set; }
    }

    // ---------- User List ----------

    public class AdminUserListItemViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminUserListViewModel
    {
        public string? Keyword { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public List<AdminUserListItemViewModel> Users { get; set; } = new();
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    // ---------- Create Admin ----------

    public class AdminCreateAdminViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

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
    }

    // ---------- Create Candidate ----------

    public class AdminCreateCandidateViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc.")]
        public string? Gender { get; set; }
    }

    // ---------- Create Recruiter ----------

    public class AdminCreateRecruiterViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

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
        public string? CompanyWebsite { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? CompanyEmail { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? CompanyPhone { get; set; }

        public string? CompanyDescription { get; set; }

        // Location fields (kept for UI, combined into city on save)
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }
        public string? WorkAddress { get; set; }
    }

    // ---------- Update User ----------

    public class AdminUpdateUserViewModel
    {
        public int Id { get; set; }

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

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        public string Role { get; set; } = "Candidate";

        public bool IsActive { get; set; }

        // Candidate-specific fields
        [MaxLength(1000)]
        public string? CandidateTitle { get; set; }

        [MaxLength(100)]
        public string? CandidateCity { get; set; }

        public DateTime? CandidateDateOfBirth { get; set; }

        public string? CandidateGender { get; set; }

        // Recruiter-specific fields
        [MaxLength(255)]
        public string? RecruiterPosition { get; set; }
    }
}
