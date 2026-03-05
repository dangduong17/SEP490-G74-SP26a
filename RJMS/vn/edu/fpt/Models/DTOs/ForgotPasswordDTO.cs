using System.ComponentModel.DataAnnotations;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}
