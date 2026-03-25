using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.vn.edu.fpt.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RJMS.Tests
{
    public class CVServiceTests
    {
        private Mock<ICVRepository> _cvRepoMock;
        private Mock<ICloudinaryService> _cloudinarySvcMock;
        private Mock<ICVRenderService> _cvRenderSvcMock;
        private CVService _cvService;

        public CVServiceTests()
        {
            _cvRepoMock = new Mock<ICVRepository>();
            _cloudinarySvcMock = new Mock<ICloudinaryService>();
            _cvRenderSvcMock = new Mock<ICVRenderService>();
            _cvService = new CVService(_cvRepoMock.Object, _cloudinarySvcMock.Object, _cvRenderSvcMock.Object);
        }

        [Fact]
        public async Task GetCandidateCvsAsync_CandidateNotFound_ReturnsEmpty()
        {
            // Arrange
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync((Candidate)null);

            // Act
            var result = await _cvService.GetCandidateCvsAsync(1);

            // Assert
            Assert.Empty(result.Cvs);
        }

        [Fact]
        public async Task GetCandidateCvsAsync_CvsFound_ReturnsList()
        {
            // Arrange
            var candidate = new Candidate { Id = 1 };
            var cvs = new List<Cv> { new Cv { Id = 1, Title = "Test CV" } };
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(candidate);
            _cvRepoMock.Setup(r => r.GetCvsByCandidateIdAsync(1)).ReturnsAsync(cvs);

            // Act
            var result = await _cvService.GetCandidateCvsAsync(1);

            // Assert
            Assert.Single(result.Cvs);
            Assert.Equal("Test CV", result.Cvs.First().Title);
        }

        [Fact]
        public async Task DeleteCvAsync_CvNotFound_ReturnsFailure()
        {
            // Arrange
            var candidate = new Candidate { Id = 1 };
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(candidate);
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(100)).ReturnsAsync((Cv)null);

            // Act
            var result = await _cvService.DeleteCvAsync(100, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Không tìm thấy CV hoặc bạn không có quyền.", result.Message);
        }

        [Fact]
        public async Task GetActiveTemplatesAsync_ReturnsTemplates()
        {
            // Arrange
            var templates = new List<CvTemplate> { new CvTemplate { Id = 1, Name = "Basic" } };
            _cvRepoMock.Setup(r => r.GetActiveTemplatesAsync()).ReturnsAsync(templates);

            // Act
            var result = await _cvService.GetActiveTemplatesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Basic", result.First().Name);
        }

        [Fact]
        public async Task SaveCvDataAsync_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var candidate = new Candidate { Id = 1 };
            var cv = new Cv { Id = 1, CandidateId = 1 };
            var cvData = new CvData { CvId = 1 };
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(candidate);
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(1)).ReturnsAsync(cv);
            _cvRepoMock.Setup(r => r.GetCvDataByCvIdAsync(1)).ReturnsAsync(cvData);

            // Act
            var result = await _cvService.SaveCvDataAsync(1, 1, "{}", "New Title");

            // Assert
            Assert.True(result.Success);
            _cvRepoMock.Verify(r => r.UpdateCvAsync(It.IsAny<Cv>()), Times.Once);
            _cvRepoMock.Verify(r => r.UpdateCvDataAsync(It.IsAny<CvData>()), Times.Once);
        }
    }
}
