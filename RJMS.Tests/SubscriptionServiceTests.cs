using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class SubscriptionServiceTests
    {
        private Mock<ISubscriptionRepository> _subRepoMock;
        private SubscriptionService _subService;

        public SubscriptionServiceTests()
        {
            _subRepoMock = new Mock<ISubscriptionRepository>();
            _subService = new SubscriptionService(_subRepoMock.Object);
        }

        // --- FUNC31: GetPlanListAsync ---

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanListAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetPlanList_UTC01_Success()
        {
            _subRepoMock.Setup(r => r.GetPlanListAsync(null, null, null, 1, 10)).ReturnsAsync(new SubscriptionListViewModel());
            var result = await _subService.GetPlanListAsync(null, null, null, 1, 10);
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanListAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetPlanList_UTC02_KeywordFilter()
        {
            _subRepoMock.Setup(r => r.GetPlanListAsync("Basic", null, null, 1, 10)).ReturnsAsync(new SubscriptionListViewModel());
            await _subService.GetPlanListAsync("Basic", null, null, 1, 10);
            _subRepoMock.Verify(r => r.GetPlanListAsync("Basic", null, null, 1, 10), Times.Once);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanListAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetPlanList_UTC03_StatusFilter()
        {
            _subRepoMock.Setup(r => r.GetPlanListAsync(null, "Active", null, 1, 10)).ReturnsAsync(new SubscriptionListViewModel());
            await _subService.GetPlanListAsync(null, "Active", null, 1, 10);
            _subRepoMock.Verify(r => r.GetPlanListAsync(null, "Active", null, 1, 10), Times.Once);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanListAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetPlanList_UTC04_RepoError()
        {
            _subRepoMock.Setup(r => r.GetPlanListAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _subService.GetPlanListAsync(null, null, null, 1, 10));
        }

        // --- FUNC32: GetPlanDetailAsync ---

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanDetailAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetPlanDetail_UTC01_Success()
        {
            _subRepoMock.Setup(r => r.GetPlanDetailAsync(1)).ReturnsAsync(new SubscriptionPlanDetailDto { Id = 1 });
            var result = await _subService.GetPlanDetailAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanDetailAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetPlanDetail_UTC02_NotFound()
        {
            _subRepoMock.Setup(r => r.GetPlanDetailAsync(99)).ReturnsAsync((SubscriptionPlanDetailDto)null);
            var result = await _subService.GetPlanDetailAsync(99);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanDetailAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetPlanDetail_UTC03_RepoError()
        {
            _subRepoMock.Setup(r => r.GetPlanDetailAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _subService.GetPlanDetailAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "GetPlanDetailAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetPlanDetail_UTC04_ZeroId()
        {
            var result = await _subService.GetPlanDetailAsync(0);
            Assert.Null(result);
        }

        // --- FUNC33: CreatePlanAsync ---

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "CreatePlanAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task CreatePlan_UTC01_Success()
        {
            _subRepoMock.Setup(r => r.CreatePlanAsync(It.IsAny<SubscriptionPlanFormViewModel>())).ReturnsAsync(1);
            var result = await _subService.CreatePlanAsync(new SubscriptionPlanFormViewModel());
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "CreatePlanAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task CreatePlan_UTC02_RepoError()
        {
            _subRepoMock.Setup(r => r.CreatePlanAsync(It.IsAny<SubscriptionPlanFormViewModel>())).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _subService.CreatePlanAsync(new SubscriptionPlanFormViewModel()));
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "CreatePlanAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task CreatePlan_UTC03_NullModel()
        {
            _subRepoMock.Setup(r => r.CreatePlanAsync(null)).ThrowsAsync(new ArgumentNullException());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _subService.CreatePlanAsync(null));
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "CreatePlanAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task CreatePlan_UTC04_ReturningZero()
        {
            _subRepoMock.Setup(r => r.CreatePlanAsync(It.IsAny<SubscriptionPlanFormViewModel>())).ReturnsAsync(0);
            var result = await _subService.CreatePlanAsync(new SubscriptionPlanFormViewModel());
            Assert.Equal(0, result);
        }

        // --- FUNC34: TogglePlanStatusAsync ---

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "TogglePlanStatusAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task TogglePlan_UTC01_Success()
        {
            _subRepoMock.Setup(r => r.TogglePlanStatusAsync(1)).ReturnsAsync(true);
            var result = await _subService.TogglePlanStatusAsync(1);
            Assert.True(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "TogglePlanStatusAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task TogglePlan_UTC02_NotFound()
        {
            _subRepoMock.Setup(r => r.TogglePlanStatusAsync(99)).ReturnsAsync(false);
            var result = await _subService.TogglePlanStatusAsync(99);
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "TogglePlanStatusAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task TogglePlan_UTC03_RepoError()
        {
            _subRepoMock.Setup(r => r.TogglePlanStatusAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _subService.TogglePlanStatusAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "TogglePlanStatusAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task TogglePlan_UTC04_ZeroId()
        {
            var result = await _subService.TogglePlanStatusAsync(0);
            Assert.False(result);
        }

        // --- FUNC35: DeletePlanAsync ---

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "DeletePlanAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task DeletePlan_UTC01_Success()
        {
            _subRepoMock.Setup(r => r.DeletePlanAsync(1)).ReturnsAsync(true);
            var result = await _subService.DeletePlanAsync(1);
            Assert.True(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "DeletePlanAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task DeletePlan_UTC02_NotFound()
        {
            _subRepoMock.Setup(r => r.DeletePlanAsync(99)).ReturnsAsync(false);
            var result = await _subService.DeletePlanAsync(99);
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "DeletePlanAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task DeletePlan_UTC03_RepoError()
        {
            _subRepoMock.Setup(r => r.DeletePlanAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _subService.DeletePlanAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Subscription")]
        [Trait("Method", "DeletePlanAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task DeletePlan_UTC04_ZeroId()
        {
            var result = await _subService.DeletePlanAsync(0);
            Assert.False(result);
        }
    }
}
