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
        public async Task<IActionResult> Candidate(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError(string.Empty, "User id is required");
                return View("Candidate", null);
            }

            var dashboard = await _dashboardService.GetDashboardAsync(userId);
            return View("Candidate", dashboard);
        }
    }
}
