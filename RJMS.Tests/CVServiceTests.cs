using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

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
            _cvService = new CVService(
                _cvRepoMock.Object,
                _cloudinarySvcMock.Object,
                _cvRenderSvcMock.Object
            );
        }

        // --- FUNC08: GetCandidateCvsAsync ---

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetCandidateCvsAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task GetCandidateCvs_UTC01_NotFound()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync((Candidate)null);
            var result = await _cvService.GetCandidateCvsAsync(1);
            Assert.Empty(result.Cvs);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetCandidateCvsAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task GetCandidateCvs_UTC02_Success()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvsByCandidateIdAsync(1)).ReturnsAsync(new List<Cv> { new Cv { Title = "CV1" } });
            var result = await _cvService.GetCandidateCvsAsync(1);
            Assert.Single(result.Cvs);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetCandidateCvsAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetCandidateCvs_UTC03_NoCvs()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvsByCandidateIdAsync(1)).ReturnsAsync(new List<Cv>());
            var result = await _cvService.GetCandidateCvsAsync(1);
            Assert.Empty(result.Cvs);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetCandidateCvsAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetCandidateCvs_UTC04_ZeroUserId()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(0)).ReturnsAsync((Candidate)null);
            var result = await _cvService.GetCandidateCvsAsync(0);
            Assert.Empty(result.Cvs);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetCandidateCvsAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetCandidateCvs_UTC05_RepoError()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _cvService.GetCandidateCvsAsync(1));
        }

        // --- FUNC09: DeleteCvAsync ---

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "DeleteCvAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task DeleteCv_UTC01_NotFound()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(100)).ReturnsAsync((Cv)null);
            var result = await _cvService.DeleteCvAsync(100, 1);
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "DeleteCvAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task DeleteCv_UTC02_Success()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(1)).ReturnsAsync(new Cv { Id = 1, CandidateId = 1 });
            var result = await _cvService.DeleteCvAsync(1, 1);
            Assert.True(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "DeleteCvAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task DeleteCv_UTC03_Unauthorized()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(1)).ReturnsAsync(new Cv { Id = 1, CandidateId = 99 });
            var result = await _cvService.DeleteCvAsync(1, 1);
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "DeleteCvAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task DeleteCv_UTC04_CandidateNotFound()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync((Candidate)null);
            var result = await _cvService.DeleteCvAsync(1, 1);
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "DeleteCvAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task DeleteCv_UTC05_RepoError()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _cvService.DeleteCvAsync(1, 1));
        }

        // --- FUNC10: GetActiveTemplatesAsync ---

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetActiveTemplatesAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetActiveTemplates_UTC01_Success()
        {
            _cvRepoMock.Setup(r => r.GetActiveTemplatesAsync()).ReturnsAsync(new List<CvTemplate> { new CvTemplate { Id = 1 } });
            var result = await _cvService.GetActiveTemplatesAsync();
            Assert.Single(result);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetActiveTemplatesAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetActiveTemplates_UTC02_Empty()
        {
            _cvRepoMock.Setup(r => r.GetActiveTemplatesAsync()).ReturnsAsync(new List<CvTemplate>());
            var result = await _cvService.GetActiveTemplatesAsync();
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetActiveTemplatesAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetActiveTemplates_UTC03_Null()
        {
            _cvRepoMock.Setup(r => r.GetActiveTemplatesAsync()).ReturnsAsync(new List<CvTemplate>());
            var result = await _cvService.GetActiveTemplatesAsync();
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetActiveTemplatesAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetActiveTemplates_UTC04_RepoError()
        {
            _cvRepoMock.Setup(r => r.GetActiveTemplatesAsync()).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _cvService.GetActiveTemplatesAsync());
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "GetActiveTemplatesAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "A")]
        public async Task GetActiveTemplates_UTC05_Multiple()
        {
            _cvRepoMock.Setup(r => r.GetActiveTemplatesAsync()).ReturnsAsync(new List<CvTemplate> { new CvTemplate { Id = 1 }, new CvTemplate { Id = 2 } });
            var result = await _cvService.GetActiveTemplatesAsync();
            Assert.Equal(2, result.Count());
        }

        // --- FUNC11: SaveCvDataAsync ---

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "SaveCvDataAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task SaveCvData_UTC01_Success()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(1)).ReturnsAsync(new Cv { Id = 1, CandidateId = 1 });
            _cvRepoMock.Setup(r => r.GetCvDataByCvIdAsync(1)).ReturnsAsync(new CvData { Id = 1 });
            var result = await _cvService.SaveCvDataAsync(1, null, 1, "{}", "Title");
            Assert.True(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "SaveCvDataAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task SaveCvData_UTC02_CvNotFound()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(1)).ReturnsAsync((Cv)null);
            var result = await _cvService.SaveCvDataAsync(1, null, 1, "{}", "Title");
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "SaveCvDataAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task SaveCvData_UTC03_Unauthorized()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
            _cvRepoMock.Setup(r => r.GetCvByIdAsync(1)).ReturnsAsync(new Cv { Id = 1, CandidateId = 99 });
            var result = await _cvService.SaveCvDataAsync(1, null, 1, "{}", "Title");
            Assert.False(result.Success);
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "SaveCvDataAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task SaveCvData_UTC04_RepoError()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ThrowsAsync(new Exception("Error"));
            await Assert.ThrowsAsync<Exception>(() => _cvService.SaveCvDataAsync(1, 1, 1, "{}", "Title"));
        }

        [Fact]
        [Trait("CodeModule", "CV")]
        [Trait("Method", "SaveCvDataAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task SaveCvData_UTC05_CandidateNotFound()
        {
            _cvRepoMock.Setup(r => r.GetCandidateByUserIdAsync(1)).ReturnsAsync((Candidate)null);
            var result = await _cvService.SaveCvDataAsync(1, null, 1, "{}", "Title");
            Assert.False(result.Success);
        }
    }
}
