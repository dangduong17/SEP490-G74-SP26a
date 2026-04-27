using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class ChatServiceTests
    {
        private FindingJobsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FindingJobsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FindingJobsDbContext(options);
        }

        // --- FUNC12: GetChatPageDataAsync ---

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "GetChatPageDataAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetChatPageData_UTC01_NoConversations()
        {
            using var context = GetInMemoryDbContext();
            var service = new ChatService(context);
            var result = await service.GetChatPageDataAsync(1);
            Assert.Empty(result.Conversations);
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "GetChatPageDataAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "A")]
        public async Task GetChatPageData_UTC02_SingleConversation()
        {
            using var context = GetInMemoryDbContext();
            context.Conversations.Add(new Conversation { Id = 1 });
            context.ConversationParticipants.Add(new ConversationParticipant { ConversationId = 1, UserId = 1 });
            await context.SaveChangesAsync();

            var service = new ChatService(context);
            var result = await service.GetChatPageDataAsync(1);
            Assert.Single(result.Conversations);
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "GetChatPageDataAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetChatPageData_UTC03_InvalidConversationId()
        {
            using var context = GetInMemoryDbContext();
            var service = new ChatService(context);
            var result = await service.GetChatPageDataAsync(1, 999);
            Assert.Empty(result.Conversations);
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "GetChatPageDataAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "A")]
        public async Task GetChatPageData_UTC04_LoadMessages()
        {
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { Id = 1, Email = "test@test.com", PasswordHash = "hash" });
            context.Conversations.Add(new Conversation { Id = 1 });
            context.ConversationParticipants.Add(new ConversationParticipant { ConversationId = 1, UserId = 1 });
            context.Messages.Add(new Message { ConversationId = 1, Content = "Hello", SenderId = 1, CreatedAt = DateTime.Now });
            await context.SaveChangesAsync();

            var service = new ChatService(context);
            var result = await service.GetChatPageDataAsync(1, 1);
            Assert.NotEmpty(result.ActiveConversation.Messages);
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "GetChatPageDataAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetChatPageData_UTC05_ZeroUserId()
        {
            using var context = GetInMemoryDbContext();
            var service = new ChatService(context);
            var result = await service.GetChatPageDataAsync(0);
            Assert.Empty(result.Conversations);
        }
    }
}
