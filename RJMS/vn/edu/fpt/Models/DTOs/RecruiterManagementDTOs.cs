namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // ─────────────────────────────────────────────────────────────
    // COMPANY LOCATION
    // ─────────────────────────────────────────────────────────────

    public class RecruiterCompanyLocationViewModel
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string? AddressLabel { get; set; }
        public bool IsPrimary { get; set; }
        public string CityName { get; set; } = "";
        public string? WardName { get; set; }
        public string? Address { get; set; }
        public int? ProvinceCode { get; set; }
        public int? WardCode { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecruiterAddLocationViewModel
    {
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }
        public string? WorkAddress { get; set; }
        public string? AddressLabel { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class CompanyLocationsPageViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = "";
        public List<RecruiterCompanyLocationViewModel> Locations { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────
    // EMPLOYEE
    // ─────────────────────────────────────────────────────────────

    public class RecruiterEmployeeListViewModel
    {
        public List<RecruiterEmployeeItemViewModel> Employees { get; set; } = new();
        public string? Keyword { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        // For create modal dropdown
        public List<RecruiterCompanyLocationViewModel> Locations { get; set; } = new();
    }

    public class RecruiterEmployeeItemViewModel
    {
        public int Id { get; set; }          // Recruiter.Id
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }   // User.IsActive
        public bool IsVerified { get; set; }
        public List<string> LocationLabels { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
    }

    public class RecruiterCreateEmployeeViewModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Position { get; set; } = "";
        public List<int> CompanyLocationIds { get; set; } = new();
    }

    public class RecruiterEditEmployeeViewModel
    {
        public int RecruiterId { get; set; }
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public List<int> CompanyLocationIds { get; set; } = new();
        // for modal
        public List<RecruiterCompanyLocationViewModel> Locations { get; set; } = new();
        public List<int> CurrentLocationIds { get; set; } = new();
    }
}
