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
        public IActionResult Create(int templateId, string title)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            // Thay vì tạo CV rỗng trong Database luôn, ta chuyển hướng sang trang Edit kèm tham số
            return RedirectToAction(nameof(Edit), new { templateId = templateId, title = title });
        }

        // ──────────────────────────────────────────────────────────────────
        // BUILDER – Editor
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? templateId, string? title)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var model = await _cvService.GetEditorViewModelAsync(id, templateId, userId.Value);
            if (model == null)
            {
                TempData["ErrorToast"] = "Không tìm thấy CV hoặc template, hoặc bạn không có quyền truy cập.";
                return RedirectToAction(nameof(Index));
            }
            
            // Nếu có truyền title từ trang Create, gán vào model cho lần đầu mở draft
            if (!string.IsNullOrWhiteSpace(title) && (!id.HasValue || id <= 0))
            {
                model.Title = title;
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int? cvId, int? templateId, string jsonData, string title)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var (success, message, newCvId) = await _cvService.SaveCvDataAsync(cvId, templateId, userId.Value, jsonData, title);
            if (success)
            {
                TempData["SuccessToast"] = message;
                return RedirectToAction(nameof(Edit), new { id = newCvId });
            }
            
            TempData["ErrorToast"] = message;
            // Nếu không thành công, tuỳ trường hợp sẽ về index hoặc reload edit
            if (cvId.HasValue && cvId > 0)
                return RedirectToAction(nameof(Edit), new { id = cvId });
            
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────────
        // PREVIEW (AJAX) – trả về HTML đã render
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PreviewHtml(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var html = await _cvService.RenderCvHtmlAsync(id);
            return Content(html, "text/html");
        }

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
