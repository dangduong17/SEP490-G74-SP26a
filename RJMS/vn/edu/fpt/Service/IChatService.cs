using RJMS.vn.edu.fpt.Models.DTOs;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IChatService
    {
        Task<ChatPageViewModel> GetChatPageDataAsync(int userId, int? activeConversationId = null);
        Task<int> StartConversationAsync(int candidateUserId, int jobId, int applicationId);
        Task<ChatMessageViewModel> SendMessageAsync(int conversationId, int senderUserId, string content);
    }
}
