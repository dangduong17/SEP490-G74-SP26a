using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;

        public JobController(IJobService jobService, IApplicationService applicationService)
        {
            _jobService = jobService;
            _applicationService = applicationService;
        }

        // ── Job List Page (GET /Job or /Job/Index) ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, int? categoryId, int? locationId, int page = 1)
        {
            if (page < 1) page = 1;

            var model = await _jobService.GetPublicJobListAsync(keyword, categoryId, locationId, page);
            ViewData["Title"] = "Tìm việc làm tốt nhất";

            return View(model);
        }

        // ── Job Detail Page (GET /Job/Detail/{id}) ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var model = await _jobService.GetJobDetailAsync(id);
            if (model == null)
            {
                TempData["ErrorToast"] = "Không tìm thấy tin tuyển dụng này.";
                return RedirectToAction("Index");
            }

            // Check if candidate already applied
            var userId = Request.Cookies["UserId"];
            var userRole = Request.Cookies["UserRole"];
            if (userRole == "Candidate" && int.TryParse(userId, out var uid))
            {
                var modalData = await _applicationService.GetApplyModalDataAsync(id, uid);
                ViewBag.AlreadyApplied = modalData?.AlreadyApplied ?? false;
            }

            ViewData["Title"] = model.Title + " | " + model.CompanyName;
            return View(model);
        }

        // ── Get Apply Modal Data (GET /Job/GetApplyData?jobId=X) ──────────────
        [HttpGet]
        public async Task<IActionResult> GetApplyData(int jobId)
        {
            var userRole = Request.Cookies["UserRole"];
            var userIdStr = Request.Cookies["UserId"];

            if (userRole != "Candidate" || !int.TryParse(userIdStr, out var userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập bằng tài khoản ứng viên." });

            var data = await _applicationService.GetApplyModalDataAsync(jobId, userId);
            if (data == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin." });

            return Json(new { success = true, data });
        }

        // ── Apply for Job (POST /Job/Apply) ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int jobId, int cvId, string? coverLetter, IFormFile? uploadFile)
        {
            var userRole = Request.Cookies["UserRole"];
            var userIdStr = Request.Cookies["UserId"];

            if (string.IsNullOrWhiteSpace(userRole) || !int.TryParse(userIdStr, out var userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập để ứng tuyển." });

            if (userRole != "Candidate")
                return Json(new { success = false, message = "Tính năng này chỉ dành cho ứng viên." });

            var result = await _applicationService.ApplyJobAsync(jobId, userId, cvId, coverLetter, uploadFile);
            return Json(result);
        }
    }
}
