using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class JobServiceTests
    {
        private Mock<IJobRepository> _jobRepoMock;
        private JobService _jobService;

        public JobServiceTests()
        {
            _jobRepoMock = new Mock<IJobRepository>();
            _jobService = new JobService(_jobRepoMock.Object);
        }

        // --- FUNC18: GetPublicJobListAsync ---

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetPublicJobListAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetPublicJobList_UTC01_Success()
        {
            _jobRepoMock.Setup(r => r.GetPublicJobListAsync(null, null, null, 1, 10)).ReturnsAsync((new List<RJMS.vn.edu.fpt.Models.Job>(), 0));
            _jobRepoMock.Setup(r => r.GetFilterDataAsync()).ReturnsAsync((new List<JobFilterCategoryDTO>(), new List<JobFilterLocationDTO>()));
            var result = await _jobService.GetPublicJobListAsync(null, null, null, 1);
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetPublicJobListAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetPublicJobList_UTC02_InvalidPage()
        {
            _jobRepoMock.Setup(r => r.GetPublicJobListAsync(null, null, null, 0, 10)).ReturnsAsync((new List<RJMS.vn.edu.fpt.Models.Job>(), 0));
            _jobRepoMock.Setup(r => r.GetFilterDataAsync()).ReturnsAsync((new List<JobFilterCategoryDTO>(), new List<JobFilterLocationDTO>()));
            var result = await _jobService.GetPublicJobListAsync(null, null, null, 0);
            _jobRepoMock.Verify(r => r.GetPublicJobListAsync(null, null, null, 0, 10), Times.Once);
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetPublicJobListAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetPublicJobList_UTC03_KeywordFilter()
        {
            _jobRepoMock.Setup(r => r.GetPublicJobListAsync("Dev", null, null, 1, 10)).ReturnsAsync((new List<RJMS.vn.edu.fpt.Models.Job>(), 0));
            _jobRepoMock.Setup(r => r.GetFilterDataAsync()).ReturnsAsync((new List<JobFilterCategoryDTO>(), new List<JobFilterLocationDTO>()));
            await _jobService.GetPublicJobListAsync("Dev", null, null, 1);
            _jobRepoMock.Verify(r => r.GetPublicJobListAsync("Dev", null, null, 1, 10), Times.Once);
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetPublicJobListAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetPublicJobList_UTC04_RepoError()
        {
            _jobRepoMock.Setup(r => r.GetPublicJobListAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _jobService.GetPublicJobListAsync(null, null, null, 1));
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetPublicJobListAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetPublicJobList_UTC05_LargePage()
        {
            _jobRepoMock.Setup(r => r.GetPublicJobListAsync(null, null, null, 1000, 10)).ReturnsAsync((new List<RJMS.vn.edu.fpt.Models.Job>(), 0));
            _jobRepoMock.Setup(r => r.GetFilterDataAsync()).ReturnsAsync((new List<JobFilterCategoryDTO>(), new List<JobFilterLocationDTO>()));
            await _jobService.GetPublicJobListAsync(null, null, null, 1000);
            _jobRepoMock.Verify(r => r.GetPublicJobListAsync(null, null, null, 1000, 10), Times.Once);
        }

        // --- FUNC19: GetJobDetailAsync ---

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetJobDetailAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task GetJobDetail_UTC01_NotFound()
        {
            _jobRepoMock.Setup(r => r.GetJobDetailAsync(1)).ReturnsAsync((RJMS.vn.edu.fpt.Models.Job)null);
            var result = await _jobService.GetJobDetailAsync(1);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetJobDetailAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task GetJobDetail_UTC02_Success()
        {
            var job = new RJMS.vn.edu.fpt.Models.Job { Id = 1, Title = "Test" };
            _jobRepoMock.Setup(r => r.GetJobDetailAsync(1)).ReturnsAsync(job);
            var result = await _jobService.GetJobDetailAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Title);
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetJobDetailAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetJobDetail_UTC03_ZeroId()
        {
            var result = await _jobService.GetJobDetailAsync(0);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetJobDetailAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetJobDetail_UTC04_RepoError()
        {
            _jobRepoMock.Setup(r => r.GetJobDetailAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _jobService.GetJobDetailAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Job")]
        [Trait("Method", "GetJobDetailAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetJobDetail_UTC05_NegativeId()
        {
            var result = await _jobService.GetJobDetailAsync(-1);
            Assert.Null(result);
        }
    }
}
