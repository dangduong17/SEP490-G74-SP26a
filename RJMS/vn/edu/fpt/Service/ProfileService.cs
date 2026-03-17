using BCrypt.Net;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public ProfileService(IProfileRepository profileRepository, ICloudinaryService cloudinaryService)
        {
            _profileRepository = profileRepository;
            _cloudinaryService = cloudinaryService;
        }

        // ── Candidate ──────────────────────────────────────────────────────────
        public async Task<UserProfileDTO?> GetPersonalProfileAsync(string userId)
        {
            return await _profileRepository.GetProfileByUserIdAsync(userId);
        }

        // ── Password ───────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> ChangePasswordAsync(
            int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return (false, "Mật khẩu mới phải có ít nhất 8 ký tự");

            if (!newPassword.Any(char.IsUpper))
                return (false, "Mật khẩu mới phải có ít nhất một chữ hoa");

            if (!newPassword.Any(char.IsDigit))
                return (false, "Mật khẩu mới phải có ít nhất một chữ số");

            if (newPassword.All(char.IsLetterOrDigit))
                return (false, "Mật khẩu mới phải có ít nhất một ký tự đặc biệt");

            var user = await _profileRepository.GetUserByIdAsync(userId);
            if (user == null)
                return (false, "Người dùng không tồn tại");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return (false, "Mật khẩu hiện tại không đúng");

            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var updated = await _profileRepository.UpdateUserPasswordAsync(userId, newHash);
            if (!updated)
                return (false, "Không thể cập nhật mật khẩu");

            return (true, "Đổi mật khẩu thành công");
        }

        // ── Recruiter ──────────────────────────────────────────────────────────
        public async Task<RecruiterProfileUpdateViewModel?> GetRecruiterProfileAsync(int userId)
        {
            return await _profileRepository.GetRecruiterProfileAsync(userId);
        }

        public async Task<bool> UpdateRecruiterProfileAsync(int userId, RecruiterProfileUpdateViewModel model)
        {
            return await _profileRepository.UpdateRecruiterProfileAsync(userId, model);
        }

        public async Task<RecruiterEditProfileViewModel?> GetRecruiterProfileForEditAsync(int userId)
        {
            return await _profileRepository.GetRecruiterProfileForEditAsync(userId);
        }

        public async Task<bool> UpdateRecruiterProfileNewAsync(int userId, RecruiterEditProfileViewModel model)
        {
            if (model.AvatarFile != null)
            {
                var avatarUrl = await _cloudinaryService.UploadImageAsync(model.AvatarFile, "avatars");
                if (avatarUrl != null) model.Avatar = avatarUrl;
            }

            if (model.CompanyLogoFile != null)
            {
                var logoUrl = await _cloudinaryService.UploadImageAsync(model.CompanyLogoFile, "logos");
                if (logoUrl != null) model.CompanyLogo = logoUrl;
            }

            return await _profileRepository.UpdateRecruiterProfileNewAsync(userId, model);
        }

        public async Task<CompanyEditProfileViewModel?> GetCompanyProfileForEditAsync(int userId)
        {
            return await _profileRepository.GetCompanyProfileForEditAsync(userId);
        }

        public async Task<bool> UpdateCompanyProfileAsync(int userId, CompanyEditProfileViewModel model)
        {
            if (model.LogoFile != null)
            {
                var logoUrl = await _cloudinaryService.UploadImageAsync(model.LogoFile, "logos");
                if (logoUrl != null) model.Logo = logoUrl;
            }
            return await _profileRepository.UpdateCompanyProfileAsync(userId, model);
        }

        // ── Candidate Edit Profile ────────────────────────────────────────────
        public async Task<CandidateEditProfileViewModel?> GetCandidateProfileForEditAsync(int userId)
        {
            return await _profileRepository.GetCandidateProfileForEditAsync(userId);
        }

        public async Task<bool> UpdateCandidateProfileAsync(int userId, CandidateEditProfileViewModel model)
        {
            return await _profileRepository.UpdateCandidateProfileAsync(userId, model);
        }

        // ── Admin Edit Profile ─────────────────────────────────────────────────
        public async Task<AdminEditProfileViewModel?> GetAdminProfileForEditAsync(int userId)
        {
            return await _profileRepository.GetAdminProfileForEditAsync(userId);
        }

        public async Task<bool> UpdateAdminProfileAsync(int userId, AdminEditProfileViewModel model)
        {
            return await _profileRepository.UpdateAdminProfileAsync(userId, model);
        }
    }
}
