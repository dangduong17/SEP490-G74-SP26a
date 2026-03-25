using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace RJMS.Tests
{
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> _authRepoMock;
        private Mock<IAdminRepository> _adminRepoMock;
        private Mock<IHttpContextAccessor> _httpContextMock;
        private Mock<IEmailService> _emailSvcMock;
        private Mock<IConfiguration> _configMock;
        private AuthService _authService;

        public AuthServiceTests()
        {
            _authRepoMock = new Mock<IAuthRepository>();
            _adminRepoMock = new Mock<IAdminRepository>();
            _httpContextMock = new Mock<IHttpContextAccessor>();
            _emailSvcMock = new Mock<IEmailService>();
            _configMock = new Mock<IConfiguration>();

            _authService = new AuthService(
                _authRepoMock.Object,
                _adminRepoMock.Object,
                _httpContextMock.Object,
                _emailSvcMock.Object,
                _configMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_UserDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "nonexistent@test.com", Password = "Password123" };
            _authRepoMock.Setup(r => r.UserExistsAsync(loginDto.Email)).ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Người dùng không tồn tại", result.Message);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "test@test.com", Password = "WrongPassword" };
            _authRepoMock.Setup(r => r.UserExistsAsync(loginDto.Email)).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password)).ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email hoặc mật khẩu không đúng", result.Message);
        }

        [Fact]
        public async Task LoginAsync_EmailNotConfirmed_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "unconfirmed@test.com", Password = "Password123" };
            var user = new User { Id = 1, Email = loginDto.Email, EmailConfirmed = false };
            
            _authRepoMock.Setup(r => r.UserExistsAsync(loginDto.Email)).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password)).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Email chưa được xác nhận", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "confirmed@test.com", Password = "Password123" };
            var user = new User { Id = 1, Email = loginDto.Email, EmailConfirmed = true, FirstName = "Test" };
            
            _authRepoMock.Setup(r => r.UserExistsAsync(loginDto.Email)).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password)).ReturnsAsync(true);
            _authRepoMock.Setup(r => r.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _authRepoMock.Setup(r => r.GetUserRoleAsync(user.Id)).ReturnsAsync("Candidate");

            var httpContext = new DefaultHttpContext();
            _httpContextMock.Setup(h => h.HttpContext).Returns(httpContext);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Đăng nhập thành công", result.Message);
            Assert.True(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDTO { Email = "existing@test.com", Password = "Password123" };
            _authRepoMock.Setup(r => r.UserExistsAsync(registerDto.Email)).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email đã được sử dụng", result.Message);
        }
    }
}
