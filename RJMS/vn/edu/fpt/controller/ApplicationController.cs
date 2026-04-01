using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly IJobApplicationService _jobApplicationService;

        public ApplicationController(IJobApplicationService jobApplicationService)
        {
            _jobApplicationService = jobApplicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Request.Cookies["UserId"];
            var role = Request.Cookies["UserRole"];

            if (string.IsNullOrWhiteSpace(userId) || role != "Candidate")
            {
                TempData["WarningToast"] = "Vui lòng đăng nhập bằng tài khoản Ứng viên.";
                return RedirectToAction("Login", "Auth");
            }

            var applications = await _jobApplicationService.GetApplicationsAsync(userId);
            return View("Index", applications);
        }
    }
}
