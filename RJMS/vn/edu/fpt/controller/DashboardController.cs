using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ICandidateDashboardService _dashboardService;

        public DashboardController(ICandidateDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Candidate()
        {
            // Lấy userId từ cookie
            var userIdStr = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để truy cập dashboard.";
                return RedirectToAction("Login", "Auth");
            }

            var dashboard = await _dashboardService.GetDashboardAsync(userId);
            return View("Candidate", dashboard);
        }
    }
}
