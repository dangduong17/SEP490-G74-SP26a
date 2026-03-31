using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RJMS.vn.edu.fpt.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinChat(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conv_{conversationId}");
        }

        public async Task LeaveChat(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conv_{conversationId}");
        }
    }
}
