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
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public List<string> BranchLabels { get; set; } = new();
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

    // ---------- Create Manager ----------

    public class AdminCreateManagerViewModel
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

        public Microsoft.AspNetCore.Http.IFormFile? CompanyLogoFile { get; set; }

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

        [MaxLength(500)]
        public string? CandidateAddress { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Mức lương hiện tại phải lớn hơn hoặc bằng 0.")]
        public decimal? CandidateCurrentSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Mức lương mong muốn phải lớn hơn hoặc bằng 0.")]
        public decimal? CandidateExpectedSalary { get; set; }

        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm phải từ 0 đến 50.")]
        public int? CandidateYearsOfExperience { get; set; }

        [MaxLength(5000)]
        public string? CandidateSummary { get; set; }

        public bool CandidateIsLookingForJob { get; set; }

        // Recruiter-specific fields
        [MaxLength(255)]
        public string? RecruiterPosition { get; set; }

        public int? RecruiterCompanyId { get; set; }

        // For company dropdown (not saved, just for display)
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Companies { get; set; }
    }

    // ========== SKILLS MANAGEMENT ==========

    public class AdminSkillListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int JobCount { get; set; }
    }

    public class AdminSkillListViewModel
    {
        public string? Keyword { get; set; }
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public List<AdminSkillListItemViewModel> Skills { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class AdminCreateSkillViewModel
    {
        [Required(ErrorMessage = "Tên kỹ năng là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên kỹ năng không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Danh mục không được vượt quá 100 ký tự.")]
        public string? Category { get; set; }
    }

    public class AdminUpdateSkillViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên kỹ năng là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên kỹ năng không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Danh mục không được vượt quá 100 ký tự.")]
        public string? Category { get; set; }
    }

    // ========== COMPANIES MANAGEMENT ==========

    public class AdminCompanyListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxCode { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsVerified { get; set; }
        public int RecruiterCount { get; set; }
        public int JobCount { get; set; }
        public int FollowerCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AdminCompanyListViewModel
    {
        public string? Keyword { get; set; }
        public string? Industry { get; set; }
        public string? VerificationStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public List<AdminCompanyListItemViewModel> Companies { get; set; } = new();
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class AdminCompanyDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string? TaxCode { get; set; }
        public string? CompanySize { get; set; }
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Description { get; set; }
        public string? Benefits { get; set; }
        public string? ProvinceName { get; set; }
        public string? WardName { get; set; }
        public string? Address { get; set; }
        public bool IsVerified { get; set; }
        public int LocationCount { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<CompanyRecruiterViewModel> Recruiters { get; set; } = new();
        public List<CompanyJobViewModel> Jobs { get; set; } = new();
    }

    public class CompanyRecruiterViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CompanyJobViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Salary { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // ========== SUBSCRIPTIONS MANAGEMENT ==========

    public class AdminSubscriptionListItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool AutoRenew { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AdminSubscriptionListViewModel
    {
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public int? PlanId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public List<AdminSubscriptionListItemViewModel> Subscriptions { get; set; } = new();
        public List<SubscriptionPlanLookupViewModel> Plans { get; set; } = new();
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class SubscriptionPlanLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class AdminSubscriptionDetailViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public int PlanDurationDays { get; set; }
        public string? PlanDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool AutoRenew { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<SubscriptionPaymentViewModel> Payments { get; set; } = new();
    }

    public class SubscriptionPaymentViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
    }

    // ========== COMPANY LOCATION MANAGEMENT ==========

    public class CompanyLocationViewModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? WardName { get; set; }
        public string? Address { get; set; }
        public string? AddressLabel { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class AdminCreateCompanyLocationViewModel
    {
        [Required(ErrorMessage = "Tên nhãn địa chỉ là bắt buộc.")]
        [MaxLength(100)]
        public string AddressLabel { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        // Location fields
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }

        [MaxLength(500)]
        public string? WorkAddress { get; set; }
    }

    // ========== EMPLOYEE MANAGEMENT ==========

    public class AdminCreateEmployeeViewModel
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

        /// <summary>ID công ty mà employee thuộc về.</summary>
        [Required(ErrorMessage = "Vui lòng chọn công ty.")]
        public int CompanyId { get; set; }

        /// <summary>Danh sách CompanyLocationId được gán cho employee (ít nhất 1).</summary>
        public List<int> CompanyLocationIds { get; set; } = new();
    }

    public class AdminEmployeeListItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<string> LocationLabels { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AdminEmployeeListViewModel
    {
        public string? Keyword { get; set; }
        public int? CompanyId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public List<AdminEmployeeListItemViewModel> Employees { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Companies { get; set; } = new();
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class AdminAssignEmployeeLocationViewModel
    {
        [Required]
        public int RecruiterId { get; set; }

        /// <summary>Danh sách CompanyLocationId mới (thay thế toàn bộ gán cũ).</summary>
        public List<int> CompanyLocationIds { get; set; } = new();
    }
}
