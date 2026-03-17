using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class CVController : Controller
    {
        private readonly ICVService _cvService;

        public CVController(ICVService cvService)
        {
            _cvService = cvService;
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
            if (!userId.HasValue)
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để xem thông tin CV.";
                return RedirectToAction("Login", "Auth");
            }

            var model = await _cvService.GetCandidateCvsAsync(userId.Value);
            return View(model);
        }
    }
}
