using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Đặt lại mật khẩu - Finding Jobs";
            var body =
                $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #00b14f;'>Đặt lại mật khẩu</h2>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Finding Jobs.</p>
                    <p>Nhấp vào liên kết dưới đây để đặt lại mật khẩu:</p>
                    <p><a href='{resetLink}' style='background-color: #00b14f; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Đặt lại mật khẩu</a></p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                    <hr/>
                    <p style='font-size: 12px; color: #666;'>Finding Jobs - Nền tảng tuyển dụng hàng đầu</p>
                </div>";

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendNewPasswordEmailAsync(string email, string newPassword)
        {
            var subject = "Mật khẩu mới cho tài khoản Finding Jobs";
            var body = $"""
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #00b14f;'>Mật khẩu mới</h2>
                    <p>Chúng tôi đã tạo mật khẩu mới cho tài khoản của bạn theo yêu cầu quên mật khẩu.</p>
                    <p><strong>Mật khẩu mới:</strong> {newPassword}</p>
                    <p>Vui lòng đăng nhập và đổi mật khẩu ngay sau khi đăng nhập để đảm bảo an toàn.</p>
                    <hr/>
                    <p style='font-size: 12px; color: #666;'>Finding Jobs - Nền tảng tuyển dụng hàng đầu</p>
                </div>
                """;

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Xác nhận email - Finding Jobs";
            var body = $"""
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #00b14f;'>Xác nhận email</h2>
                    <p>Cảm ơn bạn đã đăng ký Finding Jobs. Vui lòng nhấp vào liên kết dưới đây để xác nhận email và kích hoạt tài khoản:</p>
                    <p><a href='{confirmationLink}' style='background-color: #00b14f; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Xác nhận tài khoản</a></p>
                    <p>Nếu bạn không thực hiện đăng ký, vui lòng bỏ qua email này.</p>
                    <hr/>
                    <p style='font-size: 12px; color: #666;'>Finding Jobs - Nền tảng tuyển dụng hàng đầu</p>
                </div>
                """;

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true
        )
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var smtpUser = emailSettings["SmtpUser"];
                var smtpPass = emailSettings["SmtpPass"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser ?? "", fromName ?? "Finding Jobs"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}: {ex.Message}");
                return false;
            }
        }
    }
}
