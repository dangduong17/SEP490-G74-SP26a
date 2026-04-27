using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class ProfileServiceTests
    {
        private Mock<IProfileRepository> _profileRepoMock;
        private Mock<ICloudinaryService> _cloudinarySvcMock;
        private ProfileService _profileService;

        public ProfileServiceTests()
        {
            _profileRepoMock = new Mock<IProfileRepository>();
            _cloudinarySvcMock = new Mock<ICloudinaryService>();
            _profileService = new ProfileService(_profileRepoMock.Object, _cloudinarySvcMock.Object);
        }

        // --- FUNC27: GetPersonalProfileAsync ---

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetPersonalProfileAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetProfile_UTC01_Success()
        {
            _profileRepoMock.Setup(r => r.GetProfileByUserIdAsync("1")).ReturnsAsync(new UserProfileDTO { FullName = "Test" });
            var result = await _profileService.GetPersonalProfileAsync("1");
            Assert.NotNull(result);
            Assert.Equal("Test", result.FullName);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetPersonalProfileAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetProfile_UTC02_NotFound()
        {
            _profileRepoMock.Setup(r => r.GetProfileByUserIdAsync("99")).ReturnsAsync((UserProfileDTO)null);
            var result = await _profileService.GetPersonalProfileAsync("99");
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetPersonalProfileAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetProfile_UTC03_RepoError()
        {
            _profileRepoMock.Setup(r => r.GetProfileByUserIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _profileService.GetPersonalProfileAsync("1"));
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetPersonalProfileAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetProfile_UTC04_EmptyUserId()
        {
            _profileRepoMock.Setup(r => r.GetProfileByUserIdAsync("")).ReturnsAsync((UserProfileDTO)null);
            var result = await _profileService.GetPersonalProfileAsync("");
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetPersonalProfileAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetProfile_UTC05_NullUserId()
        {
             var result = await _profileService.GetPersonalProfileAsync(null);
             Assert.Null(result);
        }

        // --- FUNC28: ChangePasswordAsync ---

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task ChangePass_UTC01_ShortPassword()
        {
            var result = await _profileService.ChangePasswordAsync(1, "Old123!", "short");
            Assert.False(result.Success);
            Assert.Contains("ít nhất 8 ký tự", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task ChangePass_UTC02_UserNotFound()
        {
            _profileRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync((User)null);
            var result = await _profileService.ChangePasswordAsync(1, "Old123!", "NewPass123!");
            Assert.False(result.Success);
            Assert.Equal("Người dùng không tồn tại", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task ChangePass_UTC03_WrongCurrent()
        {
            var user = new User { Id = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealOld123!") };
            _profileRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
            var result = await _profileService.ChangePasswordAsync(1, "WrongOld", "NewPass123!");
            Assert.False(result.Success);
            Assert.Equal("Mật khẩu hiện tại không đúng", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "A")]
        public async Task ChangePass_UTC04_Success()
        {
            var user = new User { Id = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Old123!") };
            _profileRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
            _profileRepoMock.Setup(r => r.UpdateUserPasswordAsync(1, It.IsAny<string>())).ReturnsAsync(true);
            var result = await _profileService.ChangePasswordAsync(1, "Old123!", "NewPass123!");
            Assert.True(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task ChangePass_UTC05_NoSpecialChar()
        {
            var result = await _profileService.ChangePasswordAsync(1, "Old123!", "NewPass123");
            Assert.False(result.Success);
            Assert.Contains("ký tự đặc biệt", result.Message);
        }

        // --- FUNC29: UpdateCandidateProfileAsync ---

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "UpdateCandidateProfileAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task UpdateCandidate_UTC01_Success()
        {
            _profileRepoMock.Setup(r => r.UpdateCandidateProfileAsync(1, It.IsAny<CandidateEditProfileViewModel>())).ReturnsAsync(true);
            var result = await _profileService.UpdateCandidateProfileAsync(1, new CandidateEditProfileViewModel());
            Assert.True(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "UpdateCandidateProfileAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task UpdateCandidate_UTC02_NotFound()
        {
            _profileRepoMock.Setup(r => r.UpdateCandidateProfileAsync(99, It.IsAny<CandidateEditProfileViewModel>())).ReturnsAsync(false);
            var result = await _profileService.UpdateCandidateProfileAsync(99, new CandidateEditProfileViewModel());
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "UpdateCandidateProfileAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task UpdateCandidate_UTC03_RepoError()
        {
            _profileRepoMock.Setup(r => r.UpdateCandidateProfileAsync(It.IsAny<int>(), It.IsAny<CandidateEditProfileViewModel>())).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _profileService.UpdateCandidateProfileAsync(1, new CandidateEditProfileViewModel()));
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "UpdateCandidateProfileAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task UpdateCandidate_UTC04_NullModel()
        {
             _profileRepoMock.Setup(r => r.UpdateCandidateProfileAsync(1, null)).ReturnsAsync(false);
             var result = await _profileService.UpdateCandidateProfileAsync(1, null);
             Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "UpdateCandidateProfileAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task UpdateCandidate_UTC05_ZeroId()
        {
             var result = await _profileService.UpdateCandidateProfileAsync(0, new CandidateEditProfileViewModel());
             Assert.False(result);
        }

        // --- FUNC30: GetAdminProfileForEditAsync ---

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetAdminProfileForEditAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetAdminProfile_UTC01_Success()
        {
            _profileRepoMock.Setup(r => r.GetAdminProfileForEditAsync(1)).ReturnsAsync(new AdminEditProfileViewModel { FirstName = "Admin" });
            var result = await _profileService.GetAdminProfileForEditAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetAdminProfileForEditAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetAdminProfile_UTC02_NotFound()
        {
            _profileRepoMock.Setup(r => r.GetAdminProfileForEditAsync(99)).ReturnsAsync((AdminEditProfileViewModel)null);
            var result = await _profileService.GetAdminProfileForEditAsync(99);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetAdminProfileForEditAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetAdminProfile_UTC03_RepoError()
        {
            _profileRepoMock.Setup(r => r.GetAdminProfileForEditAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _profileService.GetAdminProfileForEditAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetAdminProfileForEditAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetAdminProfile_UTC04_ZeroId()
        {
            var result = await _profileService.GetAdminProfileForEditAsync(0);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetAdminProfileForEditAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetAdminProfile_UTC05_NegativeId()
        {
            var result = await _profileService.GetAdminProfileForEditAsync(-1);
            Assert.Null(result);
        }
    }
}
