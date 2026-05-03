using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;
using System.Text.Json;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class CVController : Controller
    {
        private readonly ICVService _cvService;
        private readonly ICloudinaryService _cloudinaryService;

        public CVController(ICVService cvService, ICloudinaryService cloudinaryService)
        {
            _cvService = cvService;
            _cloudinaryService = cloudinaryService;
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

        private IActionResult? EnsureCandidateRole()
        {
            var role = HttpContext.Request.Cookies["UserRole"];
            if (string.IsNullOrWhiteSpace(role))
            {
                return RedirectToLogin();
            }

            if (!string.Equals(role, "Candidate", StringComparison.OrdinalIgnoreCase))
            {
                TempData["WarningToast"] = "Chức năng này chỉ dành cho ứng viên.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // ──────────────────────────────────────────────────────────────────
        // GET /CV  – Danh sách CV
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
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
            if (EnsureCandidateRole() is { } redirect) return redirect;
            if (GetCurrentUserId() == null) return RedirectToLogin();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(CvUploadDTO dto)
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
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
            var userId = GetCurrentUserId();
            var role = HttpContext.Request.Cookies["UserRole"];
            var isLoggedIn = userId.HasValue;
            var isCandidate = string.Equals(role, "Candidate", StringComparison.OrdinalIgnoreCase);
            var canCreate = isLoggedIn && isCandidate;

            ViewBag.CanCreate = canCreate;
            ViewBag.IsLoggedIn = isLoggedIn;
            ViewBag.IsCandidate = isCandidate;
            var templates = await _cvService.GetActiveTemplatesAsync();
            return View(templates);
        }

        [HttpPost]
        public IActionResult Create(int templateId, string title)
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
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
            if (EnsureCandidateRole() is { } redirect) return redirect;
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
            if (EnsureCandidateRole() is { } redirect) return redirect;
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
            if (EnsureCandidateRole() is { } redirect) return redirect;
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var html = await _cvService.RenderCvHtmlAsync(id);
            return Content(html, "text/html");
        }

        [HttpPost]
        public async Task<IActionResult> Preview(int cvId, [FromBody] JsonElement jsonData)
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var html = await _cvService.RenderCvHtmlAsync(cvId, jsonData.GetRawText());
            return Content(html, "text/html");
        }

        // ──────────────────────────────────────────────────────────────────
        // UPLOAD AVATAR (base64 → Cloudinary)
        // ──────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UploadAvatar([FromBody] AvatarUploadDto dto)
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
            if (string.IsNullOrWhiteSpace(dto?.Base64)) return BadRequest();

            try
            {
                // Strip data URI prefix (data:image/png;base64,...)
                var comma = dto.Base64.IndexOf(',');
                var b64 = comma >= 0 ? dto.Base64[(comma + 1)..] : dto.Base64;
                var bytes = Convert.FromBase64String(b64);

                // Detect extension from data URI
                var ext = dto.Base64.StartsWith("data:image/png") ? "png"
                         : dto.Base64.StartsWith("data:image/webp") ? "webp"
                         : "jpg";

                using var ms = new System.IO.MemoryStream(bytes);
                var formFile = new FormFile(ms, 0, bytes.Length, "avatar", $"avatar.{ext}")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = $"image/{ext}"
                };

                var url = await _cloudinaryService.UploadImageAsync(formFile, "cv-avatars");
                if (string.IsNullOrEmpty(url)) return StatusCode(500);
                return Ok(new { url });
            }
            catch
            {
                return StatusCode(500);
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // DOWNLOAD PDF (PuppeteerSharp HTML → PDF)
        // ──────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var html = await _cvService.RenderCvHtmlAsync(id);
            if (string.IsNullOrWhiteSpace(html)) return NotFound();

            // Wrap HTML with proper styling for A4 PDF
            var fullHtml = $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{margin:0;padding:0}}.cv-page{{width:794px;margin:0 auto}}</style></head><body>{html}</body></html>";

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(fullHtml);
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new PuppeteerSharp.Media.MarginOptions { Top = "0", Right = "0", Bottom = "0", Left = "0" }
            });
            await browser.CloseAsync();

            var title = $"CV_{id}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", title);
        }

        // ──────────────────────────────────────────────────────────────────
        // DELETE
        // ──────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (EnsureCandidateRole() is { } redirect) return redirect;
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
