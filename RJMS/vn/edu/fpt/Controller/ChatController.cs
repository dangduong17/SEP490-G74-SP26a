using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.vn.edu.fpt.Hubs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        private IActionResult? GuardChatAccess()
        {
            var role = Request.Cookies["UserRole"];
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (role == "Recruiter")
            {
                TempData["WarningToast"] = "Nhà tuyển dụng không quản lý chat. Vui lòng dùng tài khoản Employee để trao đổi với ứng viên.";
                return RedirectToAction("RecruiterDashboard", "Recruiter");
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            if (GuardChatAccess() is { } redirect)
            {
                return redirect;
            }

            var userIdStr = Request.Cookies["UserId"];
            if (!int.TryParse(userIdStr, out int userId) || userId == 0) 
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _chatService.GetChatPageDataAsync(userId, id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Start(int jobId, int applicationId)
        {
            if (GuardChatAccess() is { } redirect)
            {
                return redirect;
            }

            var userIdStr = Request.Cookies["UserId"];
            if (!int.TryParse(userIdStr, out int userId) || userId == 0) 
            {
                return RedirectToAction("Login", "Auth");
            }

            var conversationId = await _chatService.StartConversationAsync(userId, jobId, applicationId);
            if (conversationId > 0)
            {
                return RedirectToAction("Index", new { id = conversationId });
            }

            return RedirectToAction("Index", "Application");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
        {
            if (GuardChatAccess() is { })
            {
                return Unauthorized();
            }

            var userIdStr = Request.Cookies["UserId"];
            if (!int.TryParse(userIdStr, out int userId) || userId == 0) 
            {
                return Unauthorized();
            }

            var messageViewModel = await _chatService.SendMessageAsync(req.ConversationId, userId, req.Content);

            // Real-time broadcast
            await _hubContext.Clients.Group($"Conv_{req.ConversationId}")
                .SendCoreAsync("ReceiveMessage", new object[] { req.ConversationId, messageViewModel.Content, messageViewModel.Time, userId });

            return Json(new { success = true, message = messageViewModel });
        }
    }

    public class SendMessageRequest
    {
        public int ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
