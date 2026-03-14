using System.Collections.Generic;
using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ChatPageViewModel> GetChatPageDataAsync(int userId)
        {
            // Mock Data for UI
            var model = new ChatPageViewModel();
            model.Conversations = new List<ChatConversationViewModel>
            {
                new ChatConversationViewModel { Id = 1, Avatar = "https://i.pravatar.cc/150?img=1", Name = "Nguyễn Thị Lan", Company = "Công ty TNHH ABC", LastMessage = "Chúng tôi rất quan tâm đến hồ sơ...", Time = "10:24", UnreadCount = 0, IsActive = true },
                new ChatConversationViewModel { Id = 2, Avatar = "https://i.pravatar.cc/150?img=11", Name = "Trần Văn Minh", Company = "Tech Solutions Co.", LastMessage = "Cám ơn bạn đã quan tâm", Time = "Hôm qua", UnreadCount = 0, IsActive = false },
                new ChatConversationViewModel { Id = 3, Avatar = "https://i.pravatar.cc/150?img=5", Name = "Phạm Thị Hương", Company = "VinaTech Group", LastMessage = "Bạn có thể tham gia phỏng vấn...", Time = "14:30", UnreadCount = 2, IsActive = false }
            };

            model.ActiveConversation = new ChatDetailViewModel
            {
                Id = 1,
                Avatar = "https://i.pravatar.cc/150?img=1",
                Name = "Nguyễn Thị Lan",
                Company = "Công ty TNHH ABC",
                IsOnline = true,
                JobApplied = "Kế toán thuế",
                JobInfo = new JobInfoViewModel
                {
                    Title = "Kế toán thuế",
                    Salary = "15-20 triệu",
                    Location = "Hà Nội",
                    Status = "Đang trao đổi"
                },
                CompanyInfo = new CompanyInfoViewModel
                {
                    Name = "Công ty TNHH ABC",
                    EmployeeCount = "100-200 nhân viên",
                    HrName = "Nguyễn Thị Lan",
                    HrTitle = "HR Manager",
                    HrAvatar = "https://i.pravatar.cc/150?img=1"
                },
                Messages = new List<ChatMessageViewModel>
                {
                    new ChatMessageViewModel { Id = 1, IsMine = false, Avatar = "https://i.pravatar.cc/150?img=1", Content = "Xin chào, chúng tôi đã xem xét hồ sơ của bạn và rất quan tâm. Bạn có thể cho chúng tôi biết thêm về kinh nghiệm làm việc của mình không?", Time = "09:15" },
                    new ChatMessageViewModel { Id = 2, IsMine = true, Avatar = "", Content = "Xin chào! Cám ơn bạn đã quan tâm. Tôi có 3 năm kinh nghiệm trong lĩnh vực kế toán thuế, đặc biệt là thuế GTGT và thuế TNDN.", Time = "09:18" },
                    new ChatMessageViewModel { Id = 3, IsMine = true, Avatar = "", Content = "Đây là CV chi tiết của tôi ạ", Time = "09:20", IsAttachment = true, FileName = "CV_NguyenVanA.pdf", FileSize = "245 KB" },
                    new ChatMessageViewModel { Id = 4, IsMine = false, Avatar = "https://i.pravatar.cc/150?img=1", Content = "Cảm ơn bạn! Chúng tôi sẽ xem xét và liên hệ lại trong vòng 2-3 ngày làm việc. Bạn có thể tham gia phỏng vấn vào tuần sau được không?", Time = "10:24" }
                }
            };

            return await Task.FromResult(model);
        }
    }
}
