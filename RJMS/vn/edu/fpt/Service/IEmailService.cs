using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink);
        Task<bool> SendNewPasswordEmailAsync(string email, string newPassword);
        Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink);
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
