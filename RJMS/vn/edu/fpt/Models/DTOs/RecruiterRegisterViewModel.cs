using System.ComponentModel.DataAnnotations;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class RecruiterRegisterViewModel
    {
        // Account info
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Personal info
        [Required(ErrorMessage = "Họ là bắt buộc")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vị trí công việc là bắt buộc")]
        public string Position { get; set; } = string.Empty;

        public string? Department { get; set; }

        // Company info
        [Required(ErrorMessage = "Tên công ty là bắt buộc")]
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyTaxCode { get; set; }

        public string? CompanySize { get; set; }

        public string? CompanyIndustry { get; set; }

        [Url(ErrorMessage = "URL website không hợp lệ")]
        public string? CompanyWebsite { get; set; }

        [EmailAddress(ErrorMessage = "Email công ty không hợp lệ")]
        public string? CompanyEmail { get; set; }

        public string? CompanyPhone { get; set; }

        public string? CompanyDescription { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với điều khoản dịch vụ")]
        public bool AgreeTerms { get; set; }
    }
}
