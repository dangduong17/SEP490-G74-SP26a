using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RJMS.vn.edu.fpt.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        /// <summary>Join group theo jobId để nhận real-time AI scoring updates.</summary>
        public async Task JoinJobGroup(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Job_{jobId}");
        }

        public async Task LeaveJobGroup(string jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Job_{jobId}");
        }
    }
}
