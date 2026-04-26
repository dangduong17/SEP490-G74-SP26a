using System;
using System.Linq;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class ChatServiceTests
    {
        private static void AssertTaskMethod(string methodName, int parameterCount)
        {
            var method = typeof(IChatService)
                .GetMethods()
                .FirstOrDefault(m =>
                    m.Name == methodName && m.GetParameters().Length == parameterCount
                );

            Assert.NotNull(method);
            Assert.True(typeof(System.Threading.Tasks.Task).IsAssignableFrom(method!.ReturnType));
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "GetChatPageDataAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public void IChatService_GetChatPageDataAsync_Exists()
        {
            AssertTaskMethod(nameof(IChatService.GetChatPageDataAsync), 2);
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "StartConversationAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public void IChatService_StartConversationAsync_Exists()
        {
            AssertTaskMethod(nameof(IChatService.StartConversationAsync), 3);
        }

        [Fact]
        [Trait("CodeModule", "Chat")]
        [Trait("Method", "SendMessageAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public void IChatService_SendMessageAsync_Exists()
        {
            AssertTaskMethod(nameof(IChatService.SendMessageAsync), 3);
        }
    }
}
