using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class AdminServiceTests
    {
        private Mock<IAdminRepository> _adminRepoMock;
        private Mock<ICloudinaryService> _cloudMock;
        private Mock<ILocationLookupService> _locMock;
        private AdminService _adminService;

        public AdminServiceTests()
        {
            _adminRepoMock = new Mock<IAdminRepository>();
            _cloudMock = new Mock<ICloudinaryService>();
            _locMock = new Mock<ILocationLookupService>();
            _adminService = new AdminService(_adminRepoMock.Object, _cloudMock.Object, _locMock.Object);
        }

        // --- FUNC01: GetDashboardAsync ---

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "GetDashboardAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetDashboard_UTC01_Success()
        {
            _adminRepoMock.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync((10, 8, 2, 1, 5, 4));
            var result = await _adminService.GetDashboardAsync();
            Assert.Equal(10, result.TotalUsers);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "GetDashboardAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetDashboard_UTC02_Empty()
        {
            _adminRepoMock.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync((0, 0, 0, 0, 0, 0));
            var result = await _adminService.GetDashboardAsync();
            Assert.Equal(0, result.TotalUsers);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "GetDashboardAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetDashboard_UTC03_RepoError()
        {
            _adminRepoMock.Setup(r => r.GetDashboardStatsAsync()).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _adminService.GetDashboardAsync());
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "GetDashboardAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetDashboard_UTC04_PositiveCheck()
        {
            _adminRepoMock.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync((100, 90, 10, 5, 45, 50));
            var result = await _adminService.GetDashboardAsync();
            Assert.True(result.ActiveUsers > 0);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "GetDashboardAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetDashboard_UTC05_Boundary()
        {
            _adminRepoMock.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync((int.MaxValue, 0, 0, 0, 0, 0));
            var result = await _adminService.GetDashboardAsync();
            Assert.Equal(int.MaxValue, result.TotalUsers);
        }

        // --- FUNC02: CreateAdminAsync ---

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateAdminAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task CreateAdminAsync_UTC01_DuplicateEmail()
        {
            var model = new AdminCreateAdminViewModel { Email = "exists@test.com", Password = "Password123!" };
            _adminRepoMock.Setup(r => r.UserEmailExistsAsync(model.Email, null)).ReturnsAsync(true);
            var result = await _adminService.CreateAdminAsync(model);
            Assert.False(result.Succeeded);
            Assert.Equal("Email đã tồn tại.", result.Errors.First().Message);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateAdminAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task CreateAdminAsync_UTC02_Success()
        {
            var model = new AdminCreateAdminViewModel { Email = "new@test.com", Password = "Password123!", FirstName = "A", LastName = "B" };
            _adminRepoMock.Setup(r => r.UserEmailExistsAsync(model.Email, null)).ReturnsAsync(false);
            _adminRepoMock.Setup(r => r.GetRoleByNameAsync("Admin")).ReturnsAsync(new Role { Id = 1 });
            var result = await _adminService.CreateAdminAsync(model);
            Assert.True(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateAdminAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task CreateAdminAsync_UTC03_ShortPassword()
        {
            var model = new AdminCreateAdminViewModel { Email = "test@test.com", Password = "short" };
            var result = await _adminService.CreateAdminAsync(model);
            Assert.False(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateAdminAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task CreateAdminAsync_UTC04_RepoError()
        {
            var model = new AdminCreateAdminViewModel { Email = "test@test.com", Password = "Password123!" };
            _adminRepoMock.Setup(r => r.UserEmailExistsAsync(model.Email, null)).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _adminService.CreateAdminAsync(model));
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateAdminAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task CreateAdminAsync_UTC05_InvalidEmailFormat()
        {
            var model = new AdminCreateAdminViewModel { Email = "not-an-email", Password = "Password123!", FirstName = "A", LastName = "B" };
            var result = await _adminService.CreateAdminAsync(model);
            Assert.True(result.Succeeded);
        }

        // --- FUNC03: CreateSkillAsync ---

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateSkillAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task CreateSkillAsync_UTC01_Success()
        {
            var model = new AdminCreateSkillViewModel { Name = "C#" };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync("C#", null)).ReturnsAsync(false);
            var result = await _adminService.CreateSkillAsync(model);
            Assert.True(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateSkillAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task CreateSkillAsync_UTC02_DuplicateName()
        {
            var model = new AdminCreateSkillViewModel { Name = "C#" };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync("C#", null)).ReturnsAsync(true);
            var result = await _adminService.CreateSkillAsync(model);
            Assert.False(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateSkillAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task CreateSkillAsync_UTC03_EmptyName()
        {
            var model = new AdminCreateSkillViewModel { Name = "" };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync("", null)).ReturnsAsync(false);
            var result = await _adminService.CreateSkillAsync(model);
            Assert.True(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateSkillAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task CreateSkillAsync_UTC04_RepoError()
        {
            var model = new AdminCreateSkillViewModel { Name = "Java" };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync("Java", null)).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _adminService.CreateSkillAsync(model));
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "CreateSkillAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task CreateSkillAsync_UTC05_LongName()
        {
            var model = new AdminCreateSkillViewModel { Name = new string('A', 200) };
            _adminRepoMock.Setup(r => r.SkillNameExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            var result = await _adminService.CreateSkillAsync(model);
            Assert.True(result.Succeeded);
        }

        // --- FUNC04: SoftDeleteUserAsync ---

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "SoftDeleteUserAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task SoftDeleteUser_UTC01_Success()
        {
            _adminRepoMock.Setup(r => r.GetUserByIdWithDetailsAsync(1)).ReturnsAsync(new User { Id = 1, IsActive = true });
            var result = await _adminService.SoftDeleteUserAsync(1);
            Assert.True(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "SoftDeleteUserAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task SoftDeleteUser_UTC02_NotFound()
        {
            _adminRepoMock.Setup(r => r.GetUserByIdWithDetailsAsync(99)).ReturnsAsync((User)null);
            var result = await _adminService.SoftDeleteUserAsync(99);
            Assert.False(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "SoftDeleteUserAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task SoftDeleteUser_UTC03_RepoError()
        {
            _adminRepoMock.Setup(r => r.GetUserByIdWithDetailsAsync(1)).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _adminService.SoftDeleteUserAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "SoftDeleteUserAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task SoftDeleteUser_UTC04_AlreadyInactive()
        {
            _adminRepoMock.Setup(r => r.GetUserByIdWithDetailsAsync(1)).ReturnsAsync(new User { Id = 1, IsActive = false });
            var result = await _adminService.SoftDeleteUserAsync(1);
            Assert.True(result.Succeeded);
        }

        [Fact]
        [Trait("CodeModule", "Admin")]
        [Trait("Method", "SoftDeleteUserAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task SoftDeleteUser_UTC05_ZeroId()
        {
            _adminRepoMock.Setup(r => r.GetUserByIdWithDetailsAsync(0)).ReturnsAsync((User)null);
            var result = await _adminService.SoftDeleteUserAsync(0);
            Assert.False(result.Succeeded);
        }
    }
}
