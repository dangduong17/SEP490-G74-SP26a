using System.ComponentModel.DataAnnotations;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
