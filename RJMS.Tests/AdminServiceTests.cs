using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.vn.edu.fpt.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RJMS.Tests
{
    public class AdminServiceTests
    {
        private Mock<IAdminRepository> _adminRepoMock;
        private Mock<ICloudinaryService> _cloudinarySvcMock;
        private AdminService _adminService;

        public AdminServiceTests()
        {
            _adminRepoMock = new Mock<IAdminRepository>();
            _cloudinarySvcMock = new Mock<ICloudinaryService>();

            _adminService = new AdminService(
                _adminRepoMock.Object,
                _cloudinarySvcMock.Object
            );
        }

        [Fact]
        public async Task GetDashboardAsync_ReturnsCorrectStats()
        {
            // Arrange
            _adminRepoMock.Setup(r => r.GetDashboardStatsAsync())
                .ReturnsAsync((100, 80, 20, 5, 60, 35));

            // Act
            var result = await _adminService.GetDashboardAsync();

            // Assert
            Assert.Equal(100, result.TotalUsers);
            Assert.Equal(80, result.ActiveUsers);
            Assert.Equal(20, result.InactiveUsers);
            Assert.Equal(5, result.TotalAdmins);
            Assert.Equal(60, result.TotalCandidates);
            Assert.Equal(35, result.TotalRecruiters);
        }

        [Fact]
        public async Task CreateAdminAsync_EmailExists_ReturnsFailure()
        {
            // Arrange
            var model = new AdminCreateAdminViewModel { Email = "admin@test.com", Password = "Password123!", FirstName = "Admin", LastName = "User" };
            _adminRepoMock.Setup(r => r.UserEmailExistsAsync(model.Email, null)).ReturnsAsync(true);

            // Act
            var result = await _adminService.CreateAdminAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Email đã tồn tại.", result.Errors.First().Message);
        }

        [Fact]
        public async Task CreateSkillAsync_SkillExists_ReturnsFailure()
        {
            // Arrange
            var model = new AdminCreateSkillViewModel { Name = "C#" };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync(model.Name, null)).ReturnsAsync(true);

            // Act
            var result = await _adminService.CreateSkillAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đã tồn tại", result.Errors.First().Message);
        }

        [Fact]
        public async Task CreateSkillAsync_ValidSkill_ReturnsSuccess()
        {
            // Arrange
            var model = new AdminCreateSkillViewModel { Name = "New Skill", Category = "IT" };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync(model.Name, null)).ReturnsAsync(false);

            // Act
            var result = await _adminService.CreateSkillAsync(model);

            // Assert
            Assert.True(result.Succeeded);
            _adminRepoMock.Verify(r => r.AddSkillAsync(It.IsAny<Skill>()), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteUserAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _adminRepoMock.Setup(r => r.GetUserByIdWithDetailsAsync(1)).ReturnsAsync((User)null);

            // Act
            var result = await _adminService.SoftDeleteUserAsync(1);

            // Assert
            Assert.False(result.Succeeded);
        }
    }
}
