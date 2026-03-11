using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using vn.edu.fpt.Utilities;
using BCrypt.Net;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IAdminRepository adminRepository,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IConfiguration configuration
        )
        {
            _authRepository = authRepository;
            _adminRepository = adminRepository;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message)> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                var userExists = await _authRepository.UserExistsAsync(loginDto.Email);
                if (!userExists)
                {
                    return (false, "Người dùng không tồn tại");
                }

                var isValid = await _authRepository.ValidateUserCredentialsAsync(
                    loginDto.Email,
                    loginDto.Password
                );

                if (!isValid)
                {
                    return (false, "Email hoặc mật khẩu không đúng");
                }

                var user = await _authRepository.GetUserByEmailAsync(loginDto.Email) as User;
                if (user != null)
                {
                    if (!(user.EmailConfirmed ?? false))
                    {
                        return (false, "Email chưa được xác nhận. Vui lòng kiểm tra hộp thư.");
                    }

                    // Manual Cookie Authentication
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Should be true in production
                        SameSite = SameSiteMode.Strict,
                        Expires = loginDto.RememberMe
                            ? DateTime.Now.AddDays(30)
                            : DateTime.Now.AddHours(2)
                    };

                    var context = _httpContextAccessor.HttpContext;
                    if (context != null)
                    {
                        var userRole = await _authRepository.GetUserRoleAsync(user.Id);
                        
                        // Debug logging
                        Console.WriteLine($"[LOGIN] Email: {user.Email}, UserId: {user.Id}, Role: {userRole}");

                        context.Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
                        context.Response.Cookies.Append("UserEmail", user.Email ?? "", cookieOptions);
                        context.Response.Cookies.Append("UserName", user.FirstName ?? "", cookieOptions);
                        context.Response.Cookies.Append("UserRole", userRole, cookieOptions);
                        
                        Console.WriteLine($"[LOGIN] Cookies set - Role cookie value: {userRole}");
                    }
                }

                return (true, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Response.Cookies.Delete("UserId");
                    context.Response.Cookies.Delete("UserEmail");
                    context.Response.Cookies.Delete("UserName");
                    context.Response.Cookies.Delete("UserRole");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(
            ForgotPasswordDTO forgotDto
        )
        {
            try
            {
                var userExists = await _authRepository.UserExistsAsync(forgotDto.Email);
                if (!userExists)
                {
                    return (false, "Email không tồn tại trong hệ thống");
                }

                var newPassword = GenerateStrongPassword();
                var hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);

                var updated = await _authRepository.UpdatePasswordHashAsync(
                    forgotDto.Email,
                    hashed
                );

                if (!updated)
                {
                    return (false, "Không thể cập nhật mật khẩu. Vui lòng thử lại.");
                }

                var emailSent = await _emailService.SendNewPasswordEmailAsync(
                    forgotDto.Email,
                    newPassword
                );

                if (!emailSent)
                {
                    return (false, "Không thể gửi email. Vui lòng thử lại.");
                }

                return (true, "Mật khẩu mới đã được gửi về email của bạn.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO registerDto)
        {
            try
            {
                var exists = await _authRepository.UserExistsAsync(registerDto.Email);
                if (exists)
                {
                    return (false, "Email đã được sử dụng");
                }

                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Phone = registerDto.PhoneNumber,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTimeHelper.NowVietnam,
                };

                var created = await _authRepository.CreateUserAsync(user);
                if (created == null)
                {
                    return (false, "Không thể tạo tài khoản. Vui lòng thử lại.");
                }

                if (
                    string.Equals(registerDto.Role, "Candidate", StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(registerDto.Role)
                )
                {
                    // Assign Candidate role
                    var candidateRole = await _adminRepository.GetRoleByNameAsync("Candidate");
                    if (candidateRole != null)
                    {
                        await _adminRepository.AddUserRoleAsync(new UserRole
                        {
                            UserId = created.Id,
                            RoleId = candidateRole.Id,
                            AssignedAt = DateTimeHelper.NowVietnam
                        });
                    }

                    var fullName = string.IsNullOrWhiteSpace(registerDto.FullName)
                        ? $"{registerDto.FirstName} {registerDto.LastName}".Trim()
                        : registerDto.FullName.Trim();

                    await _authRepository.CreateCandidateAsync(
                        new Candidate
                        {
                            UserId = created.Id,
                            FullName = fullName,
                            Phone = registerDto.PhoneNumber,
                            DateOfBirth = registerDto.DateOfBirth,
                            Gender = registerDto.Gender,
                            CreatedAt = DateTimeHelper.NowVietnam,
                            IsLookingForJob = true,
                        }
                    );
                }

                var token = GenerateEmailToken(
                    created.Email ?? string.Empty,
                    TimeSpan.FromHours(24)
                );
                var confirmLink = BuildConfirmLink(token);

                var emailSent = await _emailService.SendEmailConfirmationAsync(
                    created.Email ?? string.Empty,
                    confirmLink
                );

                if (!emailSent)
                {
                    return (
                        false,
                        "Đăng ký thành công nhưng gửi email xác nhận thất bại. Vui lòng thử lại."
                    );
                }

                return (true, "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RegisterRecruiterAsync(RecruiterRegisterViewModel registerDto)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(registerDto.PhoneNumber))
                    return (false, "Số điện thoại là bắt buộc.");
                if (string.IsNullOrWhiteSpace(registerDto.Position))
                    return (false, "Vị trí công việc là bắt buộc.");
                if (string.IsNullOrWhiteSpace(registerDto.CompanyName))
                    return (false, "Tên công ty là bắt buộc.");

                // Check if email already exists
                var exists = await _authRepository.UserExistsAsync(registerDto.Email);
                if (exists)
                {
                    return (false, "Email đã được sử dụng.");
                }

                // Check if company tax code already exists (if provided)
                if (!string.IsNullOrWhiteSpace(registerDto.CompanyTaxCode) &&
                    await _adminRepository.CompanyTaxCodeExistsAsync(registerDto.CompanyTaxCode))
                {
                    return (false, "Mã số thuế công ty đã tồn tại.");
                }

                // Create user
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Phone = registerDto.PhoneNumber,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTimeHelper.NowVietnam,
                };

                var created = await _authRepository.CreateUserAsync(user);
                if (created == null)
                {
                    return (false, "Không thể tạo tài khoản. Vui lòng thử lại.");
                }

                // Assign Recruiter role
                var recruiterRole = await _adminRepository.GetRoleByNameAsync("Recruiter");
                Console.WriteLine($"[REGISTER] RecruiterRole found: {recruiterRole != null}, RoleId: {recruiterRole?.Id}");
                
                if (recruiterRole != null)
                {
                    await _adminRepository.AddUserRoleAsync(new UserRole
                    {
                        UserId = created.Id,
                        RoleId = recruiterRole.Id,
                        AssignedAt = DateTimeHelper.NowVietnam
                    });
                    Console.WriteLine($"[REGISTER] UserRole added for UserId: {created.Id}, RoleId: {recruiterRole.Id}");
                }
                else
                {
                    Console.WriteLine($"[REGISTER ERROR] Recruiter role not found in database!");
                }

                // Create company
                var company = new Company
                {
                    Name = registerDto.CompanyName,
                    TaxCode = registerDto.CompanyTaxCode,
                    CompanySize = registerDto.CompanySize,
                    Industry = registerDto.CompanyIndustry,
                    Website = registerDto.CompanyWebsite,
                    Email = registerDto.CompanyEmail,
                    Phone = registerDto.CompanyPhone ?? registerDto.PhoneNumber,
                    Description = registerDto.CompanyDescription,
                    ProvinceCode = registerDto.ProvinceCode,
                    ProvinceName = registerDto.ProvinceName,
                    WardCode = registerDto.WardCode,
                    WardName = registerDto.WardName,
                    Address = registerDto.WorkAddress,
                    IsVerified = false, // New registrations need verification
                    CreatedAt = DateTimeHelper.NowVietnam,
                    UpdatedAt = DateTimeHelper.NowVietnam
                };
                await _adminRepository.AddCompanyAsync(company);

                // Create recruiter
                var recruiter = new Recruiter
                {
                    UserId = created.Id,
                    CompanyId = company.Id,
                    FullName = $"{registerDto.FirstName} {registerDto.LastName}".Trim(),
                    Phone = registerDto.PhoneNumber,
                    Position = registerDto.Position,
                    IsVerified = false, // New registrations need verification
                    CreatedAt = DateTimeHelper.NowVietnam
                };
                await _adminRepository.AddRecruiterAsync(recruiter);

                // Send email confirmation
                var token = GenerateEmailToken(
                    created.Email ?? string.Empty,
                    TimeSpan.FromHours(24)
                );
                var confirmLink = BuildConfirmLink(token);

                var emailSent = await _emailService.SendEmailConfirmationAsync(
                    created.Email ?? string.Empty,
                    confirmLink
                );

                if (!emailSent)
                {
                    return (
                        false,
                        "Đăng ký thành công nhưng gửi email xác nhận thất bại. Vui lòng thử lại."
                    );
                }

                return (true, "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản. Tài khoản sẽ được kích hoạt sau khi quản trị viên xác minh.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ConfirmEmailAsync(string token)
        {
            try
            {
                var email = ValidateEmailToken(token);
                if (string.IsNullOrEmpty(email))
                {
                    return (false, "Token không hợp lệ hoặc đã hết hạn.");
                }

                var confirmed = await _authRepository.SetEmailConfirmedAsync(email);
                if (!confirmed)
                {
                    return (false, "Không tìm thấy tài khoản để xác nhận.");
                }

                // Auto-assign free subscription plan to new recruiters
                await _authRepository.AssignFreeSubscriptionIfRecruiterAsync(email);

                return (true, "Xác nhận email thành công. Vui lòng đăng nhập.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        private static string GenerateStrongPassword(int length = 12)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*()_-+=[]{}";
            var all = lower + upper + digits + specials;

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = all[bytes[i] % all.Length];
            }

            return new string(chars);
        }

        private string GenerateEmailToken(string email, TimeSpan lifetime)
        {
            var expiry = DateTimeOffset.UtcNow.Add(lifetime).Ticks;
            var payload = $"{email}|{expiry}";
            var signature = Sign(payload);
            var tokenPayload = $"{payload}|{signature}";
            return Base64UrlEncode(Encoding.UTF8.GetBytes(tokenPayload));
        }

        private string BuildConfirmLink(string token)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var scheme = request?.Scheme ?? "https";
            var host = request?.Host.Value ?? "localhost";
            var encodedToken = Uri.EscapeDataString(token);
            return $"{scheme}://{host}/Auth/ConfirmEmail?token={encodedToken}";
        }

        private string ValidateEmailToken(string token)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Base64UrlDecode(token));
                var parts = decoded.Split('|');
                if (parts.Length != 3)
                {
                    return string.Empty;
                }

                var email = parts[0];
                if (!long.TryParse(parts[1], out var ticks))
                {
                    return string.Empty;
                }

                var expectedSignature = parts[2];
                var payload = $"{email}|{ticks}";
                var actualSignature = Sign(payload);

                if (!string.Equals(expectedSignature, actualSignature, StringComparison.Ordinal))
                {
                    return string.Empty;
                }

                if (DateTimeOffset.UtcNow.Ticks > ticks)
                {
                    return string.Empty;
                }

                return email;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string Sign(string data)
        {
            var secret = _configuration["Jwt:Key"] ?? "fallback-secret";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Base64UrlEncode(signatureBytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4)
            {
                case 2:
                    output += "==";
                    break;
                case 3:
                    output += "=";
                    break;
            }

            return Convert.FromBase64String(output);
        }
    }
}
