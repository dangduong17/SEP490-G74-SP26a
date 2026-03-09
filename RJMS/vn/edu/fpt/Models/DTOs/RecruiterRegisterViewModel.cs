using System.ComponentModel.DataAnnotations;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class RecruiterRegisterViewModel
    {
        // Account info
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Personal info
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vị trí công việc là bắt buộc")]
        [MaxLength(255)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        // Company info
        [Required(ErrorMessage = "Tên công ty là bắt buộc")]
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

        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với điều khoản dịch vụ")]
        public bool AgreeTerms { get; set; }
    }
}
