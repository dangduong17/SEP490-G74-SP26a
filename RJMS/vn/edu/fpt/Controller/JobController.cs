using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // ── Job List Page (GET /Job or /Job/Index) ────────────────────────────
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

            ViewData["Title"] = model.Title + " | " + model.CompanyName;
            return View(model);
        }

        // ── Apply for Job (POST /Job/Apply) ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(int jobId, int cvId)
        {
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

            TempData["SuccessToast"] = "Ứng tuyển thành công! Nhà tuyển dụng sẽ xem xét hồ sơ của bạn.";
            return RedirectToAction("Detail", new { id = jobId });
        }
    }
}
