using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> _authRepoMock;
        private Mock<IAdminRepository> _adminRepoMock;
        private Mock<IHttpContextAccessor> _httpContextMock;
        private Mock<IEmailService> _emailSvcMock;
        private Mock<IConfiguration> _configMock;
        private Mock<ILocationLookupService> _locationLookupSvcMock;
        private AuthService _authService;

        public AuthServiceTests()
        {
            _authRepoMock = new Mock<IAuthRepository>();
            _adminRepoMock = new Mock<IAdminRepository>();
            _httpContextMock = new Mock<IHttpContextAccessor>();
            _emailSvcMock = new Mock<IEmailService>();
            _configMock = new Mock<IConfiguration>();
            _locationLookupSvcMock = new Mock<ILocationLookupService>();

            _authService = new AuthService(
                _authRepoMock.Object,
                _adminRepoMock.Object,
                _httpContextMock.Object,
                _emailSvcMock.Object,
                _configMock.Object,
                _locationLookupSvcMock.Object
            );
        }

        // --- FUNC05: LoginAsync ---

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "LoginAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task LoginAsync_UTC01_NotFound()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync("none@test.com")).ReturnsAsync(false);
            var result = await _authService.LoginAsync(new LoginDTO { Email = "none@test.com" });
            Assert.False(result.Success);
            Assert.Equal("Người dùng không tồn tại", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "LoginAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task LoginAsync_UTC02_WrongPass()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync("test@test.com")).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.ValidateUserCredentialsAsync("test@test.com", "wrong")).ReturnsAsync(false);
            var result = await _authService.LoginAsync(new LoginDTO { Email = "test@test.com", Password = "wrong" });
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "LoginAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task LoginAsync_UTC03_UnconfirmedEmail()
        {
            var user = new User { Id = 1, EmailConfirmed = false };
            _authRepoMock.Setup(r => r.UserExistsAsync("un@test.com")).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.ValidateUserCredentialsAsync("un@test.com", "pass")).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.GetUserByEmailAsync("un@test.com")).ReturnsAsync(user);
            var result = await _authService.LoginAsync(new LoginDTO { Email = "un@test.com", Password = "pass" });
            Assert.False(result.Success);
            Assert.Contains("xác nhận", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "LoginAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "A")]
        public async Task LoginAsync_UTC04_Success()
        {
            var user = new User { Id = 1, EmailConfirmed = true, Email = "ok@test.com" };
            _authRepoMock.Setup(r => r.UserExistsAsync("ok@test.com")).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.ValidateUserCredentialsAsync("ok@test.com", "pass")).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.GetUserByEmailAsync("ok@test.com")).ReturnsAsync(user);
            _authRepoMock.Setup(r => r.GetUserRoleAsync(1)).ReturnsAsync("Admin");
            _httpContextMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());
            var result = await _authService.LoginAsync(new LoginDTO { Email = "ok@test.com", Password = "pass" });
            Assert.True(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "LoginAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task LoginAsync_UTC05_RepoError()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Fail"));
            var result = await _authService.LoginAsync(new LoginDTO { Email = "err@test.com" });
            Assert.False(result.Success);
            Assert.Contains("Lỗi", result.Message);
        }

        // --- FUNC06: RegisterAsync ---

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "RegisterAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task RegisterAsync_UTC01_DuplicateEmail()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync("ex@test.com")).ReturnsAsync(true);
            var result = await _authService.RegisterAsync(new RegisterDTO { Email = "ex@test.com" });
            Assert.False(result.Success);
            Assert.Equal("Email đã được sử dụng", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "RegisterAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task RegisterAsync_UTC02_Success()
        {
            var dto = new RegisterDTO { Email = "new@test.com", Password = "pass", FirstName = "A", LastName = "B" };
            _authRepoMock.Setup(r => r.UserExistsAsync("new@test.com")).ReturnsAsync(false);
            _authRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(new User { Id = 1, Email = "new@test.com" });
            _adminRepoMock.Setup(r => r.GetRoleByNameAsync("Candidate")).ReturnsAsync(new Role { Id = 2 });
            _emailSvcMock.Setup(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _configMock.Setup(c => c["Jwt:Key"]).Returns("secret-key-12345678901234567890");
            var result = await _authService.RegisterAsync(dto);
            Assert.True(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "RegisterAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task RegisterAsync_UTC03_CreateFail()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _authRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync((User)null);
            var result = await _authService.RegisterAsync(new RegisterDTO { Email = "fail@test.com" });
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "RegisterAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task RegisterAsync_UTC04_EmailSendFail()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _authRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(new User { Id = 1, Email = "ok@test.com" });
            _emailSvcMock.Setup(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _configMock.Setup(c => c["Jwt:Key"]).Returns("secret");
            var result = await _authService.RegisterAsync(new RegisterDTO { Email = "ok@test.com" });
            Assert.False(result.Success);
            Assert.Contains("gửi email xác nhận thất bại", result.Message);
        }

        [Fact]
        [Trait("CodeModule", "Authentication")]
        [Trait("Method", "RegisterAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task RegisterAsync_UTC05_RepoError()
        {
            _authRepoMock.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ThrowsAsync(new Exception("DB Down"));
            var result = await _authService.RegisterAsync(new RegisterDTO { Email = "err@test.com" });
            Assert.False(result.Success);
        }
    }
}
