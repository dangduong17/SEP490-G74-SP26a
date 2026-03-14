using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            // Optional: require login realistically. Currently mocked.
            // if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var dummyUserId = userId ?? 1; // Fallback to 1 for demo
            var model = await _chatService.GetChatPageDataAsync(dummyUserId);
            
            return View(model);
        }
    }
}
