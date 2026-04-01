using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IApplicationService
    {
        Task<ApplyModalData?> GetApplyModalDataAsync(int jobId, int userId);
        Task<ApplyJobResponse> ApplyJobAsync(int jobId, int userId, int cvId, string? coverLetter, IFormFile? uploadFile);
        Task<List<NotificationDto>> GetNotificationsAsync(int userId, int take = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkReadAsync(int notificationId, int userId);
        Task MarkAllReadAsync(int userId);
    }
}
