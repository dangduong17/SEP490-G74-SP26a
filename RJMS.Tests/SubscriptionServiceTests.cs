using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.vn.edu.fpt.Models.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RJMS.Tests
{
    public class SubscriptionServiceTests
    {
        private Mock<ISubscriptionRepository> _repoMock;
        private SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _repoMock = new Mock<ISubscriptionRepository>();
            _service = new SubscriptionService(_repoMock.Object);
        }

        [Fact]
        public async Task GetPlanListAsync_CallsRepository()
        {
            // Act
            await _service.GetPlanListAsync(null, null, null, 1, 10);

            // Assert
            _repoMock.Verify(r => r.GetPlanListAsync(null, null, null, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetPlanDetailAsync_ReturnsData()
        {
            // Arrange
            var detail = new SubscriptionPlanDetailDto { Id = 1, Name = "Plan 1" };
            _repoMock.Setup(r => r.GetPlanDetailAsync(1)).ReturnsAsync(detail);

            // Act
            var result = await _service.GetPlanDetailAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Plan 1", result.Name);
        }

        [Fact]
        public async Task CreatePlanAsync_CallsRepoAndReturnsId()
        {
            // Arrange
            var model = new SubscriptionPlanFormViewModel { Name = "New Plan" };
            _repoMock.Setup(r => r.CreatePlanAsync(model)).ReturnsAsync(5);

            // Act
            var result = await _service.CreatePlanAsync(model);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task TogglePlanStatusAsync_ReturnsResult()
        {
            // Arrange
            _repoMock.Setup(r => r.TogglePlanStatusAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.TogglePlanStatusAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeletePlanAsync_ReturnsResult()
        {
            // Arrange
            _repoMock.Setup(r => r.DeletePlanAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.DeletePlanAsync(1);

            // Assert
            Assert.True(result);
        }
    }
}
