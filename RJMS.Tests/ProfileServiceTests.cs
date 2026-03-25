using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

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

        [Fact]
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
        public async Task ChangePasswordAsync_ShortPassword_ReturnsFailure()
        {
            // Act
            var result = await _profileService.ChangePasswordAsync(1, "OldPass123!", "short");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Mật khẩu mới phải có ít nhất 8 ký tự", result.Message);
        }

        [Fact]
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
