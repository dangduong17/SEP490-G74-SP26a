using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;
using System.Text.Json;

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
            var s = HttpContext.Request.Cookies["UserId"];
            return int.TryParse(s, out var id) ? id : null;
        }

        private IActionResult RedirectToLogin()
        {
            TempData["ErrorToast"] = "Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Auth");
        }

        // ──────────────────────────────────────────────────────────────────
        // GET /CV  – Danh sách CV
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var model = await _cvService.GetCandidateCvsAsync(userId.Value);
            return View(model);
        }

        // ──────────────────────────────────────────────────────────────────
        // UPLOAD
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Upload()
        {
            if (GetCurrentUserId() == null) return RedirectToLogin();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(CvUploadDTO dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var (success, message, _) = await _cvService.UploadCvAsync(userId.Value, dto);
            if (success)
            {
                TempData["SuccessToast"] = message;
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorToast"] = message;
            return View(dto);
        }

        // ──────────────────────────────────────────────────────────────────
        // BUILDER – Chọn template
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (GetCurrentUserId() == null) return RedirectToLogin();
            var templates = await _cvService.GetActiveTemplatesAsync();
            return View(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int templateId, string title)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var (success, message, cvId) = await _cvService.CreateBuilderCvAsync(userId.Value, templateId, title);
            if (success)
                return RedirectToAction(nameof(Edit), new { id = cvId });

            TempData["ErrorToast"] = message;
            return RedirectToAction(nameof(Create));
        }

        // ──────────────────────────────────────────────────────────────────
        // BUILDER – Editor
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var model = await _cvService.GetEditorViewModelAsync(id, userId.Value);
            if (model == null)
            {
                TempData["ErrorToast"] = "Không tìm thấy CV hoặc bạn không có quyền truy cập.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int cvId, string jsonData, string title)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var (success, message) = await _cvService.SaveCvDataAsync(cvId, userId.Value, jsonData, title);
            if (success)
                TempData["SuccessToast"] = message;
            else
                TempData["ErrorToast"] = message;

            return RedirectToAction(nameof(Edit), new { id = cvId });
        }

        // ──────────────────────────────────────────────────────────────────
        // PREVIEW (AJAX) – trả về HTML đã render
        // ──────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Preview(int cvId, [FromBody] JsonElement jsonData)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var html = await _cvService.RenderCvHtmlAsync(cvId, jsonData.GetRawText());
            return Content(html, "text/html");
        }

        // ──────────────────────────────────────────────────────────────────
        // DELETE
        // ──────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var (success, message) = await _cvService.DeleteCvAsync(id, userId.Value);
            if (success)
                TempData["SuccessToast"] = message;
            else
                TempData["ErrorToast"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
