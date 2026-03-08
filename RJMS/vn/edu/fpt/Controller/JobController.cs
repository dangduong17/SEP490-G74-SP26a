using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class JobController : Controller
    {
        // ── Job List Page (GET /Job or /Job/Index) ────────────────────────────
        [HttpGet]
        public IActionResult Index(string? keyword, string? category, string? location, string? jobType, int page = 1)
        {
            // TODO: Integrate with JobService to get real data
            ViewData["Title"] = "Danh sách việc làm";
            
            // For now, return view with empty model
            // Later: var model = await _jobService.GetJobListAsync(keyword, category, location, jobType, page);
            return View();
        }

        // ── Job Detail Page (GET /Job/Detail/{id}) ─────────────────────────────
        [HttpGet]
        public IActionResult Detail(int id)
        {
            // TODO: Integrate with JobService to get job detail
            ViewData["Title"] = "Chi tiết việc làm";
            
            // For now, return view
            // Later: var model = await _jobService.GetJobDetailAsync(id);
            // if (model == null) return NotFound();
            return View();
        }

        // ── Apply for Job (POST /Job/Apply) ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(int jobId, int cvId)
        {
            // Check if user is logged in and is candidate
            var userRole = HttpContext.Request.Cookies["UserRole"];
            var userId = HttpContext.Request.Cookies["UserId"];

            if (string.IsNullOrWhiteSpace(userRole) || string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để ứng tuyển";
                return RedirectToAction("Login", "Auth");
            }

            if (userRole != "Candidate")
            {
                TempData["ErrorToast"] = "Tính năng này chỉ dành cho ứng viên";
                return RedirectToAction("Index", "Home");
            }

            // TODO: Integrate with ApplicationService to create application
            // await _applicationService.CreateApplicationAsync(jobId, int.Parse(userId), cvId);
            
            TempData["SuccessToast"] = "[Demo] Ứng tuyển thành công! Nhà tuyển dụng sẽ xem xét hồ sơ của bạn.";
            return RedirectToAction("Detail", new { id = jobId });
        }
    }
}
