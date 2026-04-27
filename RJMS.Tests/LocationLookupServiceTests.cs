using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class LocationLookupServiceTests
    {
        private Mock<HttpMessageHandler> _handlerMock;
        private LocationLookupService _service;

        public LocationLookupServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.test")
            };
            _service = new LocationLookupService(httpClient);
        }

        private void SetupResponse(string content, HttpStatusCode status = HttpStatusCode.OK)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(content)
                });
        }

        // --- FUNC20: GetProvincesAsync ---

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetProvinces_UTC01_Success()
        {
            SetupResponse("[{\"code\": 1, \"name\": \"Hanoi\"}]");
            var result = await _service.GetProvincesAsync();
            Assert.Single(result);
            Assert.Equal("Hanoi", result[0].Name);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetProvinces_UTC02_HttpError()
        {
            SetupResponse("", HttpStatusCode.InternalServerError);
            var result = await _service.GetProvincesAsync();
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetProvinces_UTC03_EmptyJson()
        {
            SetupResponse("[]");
            var result = await _service.GetProvincesAsync();
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "A")]
        public async Task GetProvinces_UTC04_MultipleResults()
        {
            SetupResponse("[{\"code\": 1, \"name\": \"A\"}, {\"code\": 2, \"name\": \"B\"}]");
            var result = await _service.GetProvincesAsync();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetProvinces_UTC05_InvalidJson()
        {
            SetupResponse("invalid");
            var result = await _service.GetProvincesAsync();
            Assert.Empty(result);
        }

        // --- FUNC21: GetWardsByProvinceCodeAsync ---

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetWards_UTC01_Success()
        {
            SetupResponse("{\"wards\": [{\"code\": 10, \"name\": \"Ward1\"}]}");
            var result = await _service.GetWardsByProvinceCodeAsync(1);
            Assert.Single(result);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetWards_UTC02_NoData()
        {
            SetupResponse("{}");
            var result = await _service.GetWardsByProvinceCodeAsync(1);
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetWards_UTC03_HttpError()
        {
            SetupResponse("", HttpStatusCode.NotFound);
            var result = await _service.GetWardsByProvinceCodeAsync(1);
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetWards_UTC04_InvalidCode()
        {
            SetupResponse("null");
            var result = await _service.GetWardsByProvinceCodeAsync(999);
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "Location")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "A")]
        public async Task GetWards_UTC05_FallbackToDistricts()
        {
            SetupResponse("{\"districts\": [{\"code\": 20, \"name\": \"Dist1\"}]}");
            var result = await _service.GetWardsByProvinceCodeAsync(1);
            Assert.Single(result);
        }
    }
}
