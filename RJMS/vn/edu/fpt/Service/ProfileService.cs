using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using BCrypt.Net;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<UserProfileDTO?> GetPersonalProfileAsync(string userId)
        {
            var profile = await _profileRepository.GetProfileByUserIdAsync(userId);
            return profile;
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            // Validate new password format
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                return (false, "Mật khẩu mới phải có ít nhất 8 ký tự");
            }

            if (!newPassword.Any(char.IsUpper))
            {
                return (false, "Mật khẩu mới phải có ít nhất một chữ hoa");
            }

            if (!newPassword.Any(char.IsDigit))
            {
                return (false, "Mật khẩu mới phải có ít nhất một chữ số");
            }

            if (newPassword.All(char.IsLetterOrDigit))
            {
                return (false, "Mật khẩu mới phải có ít nhất một ký tự đặc biệt");
            }

            // Get user
            var user = await _profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, "Người dùng không tồn tại");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return (false, "Mật khẩu hiện tại không đúng");
            }

            // Hash new password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Update password
            var updated = await _profileRepository.UpdateUserPasswordAsync(userId, newPasswordHash);
            if (!updated)
            {
                return (false, "Không thể cập nhật mật khẩu");
            }

            return (true, "Đổi mật khẩu thành công");
        }
    }
}
