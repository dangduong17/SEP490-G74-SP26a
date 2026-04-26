using System.Collections.Generic;
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
        private Mock<HttpMessageHandler> _msgHandlerMock;
        private HttpClient _httpClient;
        private LocationLookupService _service;

        public LocationLookupServiceTests()
        {
            _msgHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_msgHandlerMock.Object)
            {
                BaseAddress = new System.Uri("https://provinces.open-api.vn"),
            };
            _service = new LocationLookupService(_httpClient);
        }

        [Fact]
        [Trait("CodeModule", "LocationLookup")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetProvincesAsync_Success_ReturnsList()
        {
            // Arrange
            var json = "[{\"code\": 1, \"name\": \"Hà Nội\"}]";
            _msgHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(json),
                    }
                );

            // Act
            var result = await _service.GetProvincesAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("Hà Nội", result[0].Name);
        }

        [Fact]
        [Trait("CodeModule", "LocationLookup")]
        [Trait("Method", "GetProvincesAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetProvincesAsync_Error_ReturnsEmpty()
        {
            // Arrange
            _msgHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }
                );

            // Act
            var result = await _service.GetProvincesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "LocationLookup")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetWardsByProvinceCodeAsync_ValidCode_ReturnsWards()
        {
            // Arrange
            var json = "{\"wards\": [{\"code\": 1, \"name\": \"Ward 1\"}]}";
            _msgHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(json),
                    }
                );

            // Act
            var result = await _service.GetWardsByProvinceCodeAsync(1);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("Ward 1", result[0].Name);
        }

        [Fact]
        [Trait("CodeModule", "LocationLookup")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task GetWardsByProvinceCodeAsync_FallbackToDistricts_ReturnsList()
        {
            // Arrange
            var json = "{\"districts\": [{\"code\": 2, \"name\": \"District 2\"}]}";
            _msgHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(json),
                    }
                );

            // Act
            var result = await _service.GetWardsByProvinceCodeAsync(1);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("District 2", result[0].Name);
        }

        [Fact]
        [Trait("CodeModule", "LocationLookup")]
        [Trait("Method", "GetWardsByProvinceCodeAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetWardsByProvinceCodeAsync_HttpError_ReturnsEmptyList()
        {
            // Arrange
            _msgHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            // Act
            var result = await _service.GetWardsByProvinceCodeAsync(1);

            // Assert
            Assert.Empty(result);
        }
    }
}
