using Moq;
using Xunit;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.vn.edu.fpt.Models.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RJMS.Tests
{
    public class ChatServiceTests
    {
        private Mock<IChatRepository> _chatRepoMock;
        private ChatService _chatService;

        public ChatServiceTests()
        {
            _chatRepoMock = new Mock<IChatRepository>();
            _chatService = new ChatService(_chatRepoMock.Object);
        }

        [Fact]
        public async Task GetChatPageDataAsync_ReturnsViewModelWithConversations()
        {
            // Act
            var result = await _chatService.GetChatPageDataAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Conversations);
        }

        [Fact]
        public async Task GetChatPageDataAsync_HasActiveConversation()
        {
            // Act
            var result = await _chatService.GetChatPageDataAsync(1);

            // Assert
            Assert.NotNull(result.ActiveConversation);
            Assert.Equal(1, result.ActiveConversation.Id);
        }

        [Fact]
        public async Task GetChatPageDataAsync_ContainsMessages()
        {
            // Act
            var result = await _chatService.GetChatPageDataAsync(1);

            // Assert
            Assert.NotEmpty(result.ActiveConversation.Messages);
        }

        [Fact]
        public async Task GetChatPageDataAsync_IncludesJobInfo()
        {
            // Act
            var result = await _chatService.GetChatPageDataAsync(1);

            // Assert
            Assert.NotNull(result.ActiveConversation.JobInfo);
            Assert.Equal("Kế toán thuế", result.ActiveConversation.JobInfo.Title);
        }

        [Fact]
        public async Task GetChatPageDataAsync_IncludesCompanyInfo()
        {
            // Act
            var result = await _chatService.GetChatPageDataAsync(1);

            // Assert
            Assert.NotNull(result.ActiveConversation.CompanyInfo);
        }
    }
}
