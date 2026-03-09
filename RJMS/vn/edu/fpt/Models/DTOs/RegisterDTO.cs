using System.ComponentModel.DataAnnotations;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string? Gender { get; set; }

        public string? Role { get; set; } = "Candidate";
    }
}
