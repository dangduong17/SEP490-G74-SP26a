using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IApplicationService _applicationService;

        public NotificationController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        private int? GetCurrentUserId()
        {
            var str = Request.Cookies["UserId"];
            return int.TryParse(str, out var id) ? id : null;
        }

        // GET: /Notification/GetList
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Json(new { notifications = new object[0], unreadCount = 0 });

            var list = await _applicationService.GetNotificationsAsync(userId.Value, 20);
            var unread = await _applicationService.GetUnreadCountAsync(userId.Value);

            return Json(new { notifications = list, unreadCount = unread });
        }

        // GET: /Notification/UnreadCount
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Json(new { count = 0 });
            var count = await _applicationService.GetUnreadCountAsync(userId.Value);
            return Json(new { count });
        }

        // POST: /Notification/MarkRead
        [HttpPost]
        public async Task<IActionResult> MarkRead([FromBody] MarkReadRequest req)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Json(new { success = false });
            var ok = await _applicationService.MarkReadAsync(req.Id, userId.Value);
            return Json(new { success = ok });
        }

        // POST: /Notification/MarkAllRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Json(new { success = false });
            await _applicationService.MarkAllReadAsync(userId.Value);
            return Json(new { success = true });
        }
    }

    public class MarkReadRequest
    {
        public int Id { get; set; }
    }
}
