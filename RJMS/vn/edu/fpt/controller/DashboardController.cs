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
        public async Task<IActionResult> Candidate(Guid? userId)
        {
            var dashboard = await _dashboardService.GetDashboardAsync(userId ?? Guid.Empty);
            return View("Candidate", dashboard);
        }
    }
}
