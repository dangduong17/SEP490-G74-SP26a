using RJMS.vn.edu.fpt.Models.DTOs;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IChatService
    {
        Task<ChatPageViewModel> GetChatPageDataAsync(int userId);
    }
}
