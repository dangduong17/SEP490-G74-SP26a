using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using System;

namespace RJMS.Tests
{
    public class CVRenderServiceTests
    {
        private CVRenderService _renderService;

        public CVRenderServiceTests()
        {
            _renderService = new CVRenderService();
        }

        [Fact]
        public void Render_EmptyData_ReturnsWrappedDocument()
        {
            // Act
            var result = _renderService.Render("{}", "{}");

            // Assert
            Assert.Contains("cv-document", result);
        }

        [Fact]
        public void Render_CustomData_ReplacesPlaceholders()
        {
            // Arrange
            var dataJson = "{\"FullName\": \"John Doe\", \"Position\": \"Software Engineer\"}";
            
            // Act
            var result = _renderService.Render("", dataJson);

            // Assert
            Assert.Contains("John Doe", result);
            Assert.Contains("Software Engineer", result);
        }

        [Fact]
        public void Render_InvalidJson_HandlesExceptionGracefully()
        {
            // Act & Assert
            // The service code uses JsonSerializer.Deserialize which might throw or return null.
            // In the implementation: JsonSerializer.Deserialize<CvDataModel>(dataJson, _opts) ?? new CvDataModel();
            // If it throws, we should see what happens.
            
            var result = _renderService.Render("invalid", "invalid");
            Assert.NotNull(result);
            Assert.Contains("cv-document", result);
        }

        [Fact]
        public void Render_SkillsList_RendersAllSkills()
        {
            // Arrange
            var dataJson = "{\"Skills\": \"C#, Java, Python\"}";

            // Act
            var result = _renderService.Render("", dataJson);

            // Assert
            Assert.Contains("C#", result);
            Assert.Contains("Java", result);
            Assert.Contains("Python", result);
        }

        [Fact]
        public void Render_Experience_RendersCompany()
        {
            // Arrange
            var dataJson = "{\"Experiences\": [{\"Company\": \"Google\", \"Role\": \"Dev\", \"Period\": \"2020-2023\", \"Description\": \"Worked hard\"}]}";

            // Act
            var result = _renderService.Render("", dataJson);

            // Assert
            Assert.Contains("Google", result);
            Assert.Contains("Worked hard", result);
        }
    }
}
