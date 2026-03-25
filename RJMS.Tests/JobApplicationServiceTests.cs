using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RJMS.Tests
{
    public class JobApplicationServiceTests
    {
        private Mock<IJobApplicationRepository> _repoMock;
        private JobApplicationService _service;

        public JobApplicationServiceTests()
        {
            _repoMock = new Mock<IJobApplicationRepository>();
            _service = new JobApplicationService(_repoMock.Object);
        }

        [Fact]
        public async Task GetApplicationsAsync_ValidUserId_ReturnsList()
        {
            // Arrange
            var apps = new List<JobApplicationDTO> { new JobApplicationDTO { PositionTitle = "Dev" } };
            _repoMock.Setup(r => r.GetApplicationsAsync("1")).ReturnsAsync(apps);

            // Act
            var result = await _service.GetApplicationsAsync("1");

            // Assert
            Assert.NotEmpty(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetApplicationsAsync_NoApps_ReturnsEmpty()
        {
            // Arrange
            _repoMock.Setup(r => r.GetApplicationsAsync("1")).ReturnsAsync(new List<JobApplicationDTO>());

            // Act
            var result = await _service.GetApplicationsAsync("1");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetApplicationsAsync_VerifyRepositoryCall()
        {
            // Act
            await _service.GetApplicationsAsync("1");

            // Assert
            _repoMock.Verify(r => r.GetApplicationsAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetApplicationsAsync_NullUserId_HandlesRepositoryResponse()
        {
            // Arrange
            _repoMock.Setup(r => r.GetApplicationsAsync(null)).ReturnsAsync((List<JobApplicationDTO>)null);

            // Act
            var result = await _service.GetApplicationsAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetApplicationsAsync_MultipleApps_ReturnsAll()
        {
            // Arrange
            var apps = new List<JobApplicationDTO> 
            { 
                new JobApplicationDTO { PositionTitle = "Dev" },
                new JobApplicationDTO { PositionTitle = "QA" }
            };
            _repoMock.Setup(r => r.GetApplicationsAsync("1")).ReturnsAsync(apps);

            // Act
            var result = await _service.GetApplicationsAsync("1");

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}
