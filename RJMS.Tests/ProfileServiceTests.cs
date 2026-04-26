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
            _profileService = new ProfileService(
                _profileRepoMock.Object,
                _cloudinarySvcMock.Object
            );
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetPersonalProfileAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetPersonalProfileAsync_ValidUserId_ReturnsProfile()
        {
            // Arrange
            var profile = new UserProfileDTO { FullName = "Test User" };
            _profileRepoMock.Setup(r => r.GetProfileByUserIdAsync("1")).ReturnsAsync(profile);

            // Act
            var result = await _profileService.GetPersonalProfileAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test User", result.FullName);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task ChangePasswordAsync_ShortPassword_ReturnsFailure()
        {
            // Act
            var result = await _profileService.ChangePasswordAsync(1, "OldPass123!", "short");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Mật khẩu mới phải có ít nhất 8 ký tự", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "ChangePasswordAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task ChangePasswordAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            _profileRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync((User)null);

            // Act
            var result = await _profileService.ChangePasswordAsync(1, "OldPass123!", "NewPass123!");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Người dùng không tồn tại", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "UpdateCandidateProfileAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task UpdateCandidateProfileAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var model = new CandidateEditProfileViewModel();
            _profileRepoMock.Setup(r => r.UpdateCandidateProfileAsync(1, model)).ReturnsAsync(true);

            // Act
            var result = await _profileService.UpdateCandidateProfileAsync(1, model);

            // Assert
            Assert.True(result);
        }

        [Fact]
        [Trait("CodeModule", "Profile")]
        [Trait("Method", "GetAdminProfileForEditAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetAdminProfileForEditAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var profile = new AdminEditProfileViewModel();
            _profileRepoMock.Setup(r => r.GetAdminProfileForEditAsync(1)).ReturnsAsync(profile);

            // Act
            var result = await _profileService.GetAdminProfileForEditAsync(1);

            // Assert
            Assert.NotNull(result);
        }
    }
}
