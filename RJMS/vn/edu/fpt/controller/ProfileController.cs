using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Service;
using vn.edu.fpt.dto;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // ── Helper: đọc UserId từ cookie ──────────────────────────────────────
        private int? GetCurrentUserId()
        {
            var raw = Request.Cookies["UserId"];
            return int.TryParse(raw, out var id) ? id : null;
        }

        private IActionResult? RequireRecruiter()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Recruiter")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ── Candidate profile (giữ nguyên) ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PersonalProfile(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError(string.Empty, "User id is required");
                return View("PersonalProfile", new UserProfileDTO());
            }

            var profile = await _profileService.GetPersonalProfileAsync(userId);
            if (profile == null)
            {
                ModelState.AddModelError(string.Empty, "Profile not found");
                profile = new UserProfileDTO();
            }

            return View("PersonalProfile", profile);
        }

<<<<<<< Updated upstream
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Check if user is logged in
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (string.IsNullOrWhiteSpace(userIdStr))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để đổi mật khẩu";
                return RedirectToAction("Login", "Auth");
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Check if user is logged in
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để đổi mật khẩu";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message) = await _profileService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (success)
            {
                TempData["SuccessToast"] = message;
                return RedirectToAction("ChangePassword");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
=======
        // ── Recruiter – Chỉnh sửa hồ sơ (GET) ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = await _profileService.GetRecruiterProfileAsync(userId.Value);
            if (model == null)
            {
                // Recruiter chưa có profile đầy đủ — trả về form trống
                model = new RecruiterProfileUpdateViewModel();
            }

            ViewData["Title"] = "Chỉnh sửa hồ sơ nhà tuyển dụng";
            return View("~/Views/Recruiter/EditProfile.cshtml", model);
        }

        // ── Recruiter – Chỉnh sửa hồ sơ (POST) ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(RecruiterProfileUpdateViewModel model)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Chỉnh sửa hồ sơ nhà tuyển dụng";

            if (!ModelState.IsValid)
                return View("~/Views/Recruiter/EditProfile.cshtml", model);

            var success = await _profileService.UpdateRecruiterProfileAsync(userId.Value, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Cập nhật thất bại. Vui lòng thử lại.");
                return View("~/Views/Recruiter/EditProfile.cshtml", model);
            }

            // Cập nhật lại cookie UserName để header hiển thị đúng
            var fullName = $"{model.FirstName} {model.LastName}".Trim();
            Response.Cookies.Append("UserName", fullName, new CookieOptions
            {
                HttpOnly = false,
                Secure   = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires  = DateTimeOffset.UtcNow.AddDays(30)
            });

            TempData["SuccessToast"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("RecruiterDashboard", "Recruiter");
        }

        // ── Stub: Info (dùng cho link trong header) ────────────────────────────
        [HttpGet]
        public IActionResult Info()
        {
            var role = Request.Cookies["UserRole"];
            // Recruiter → trang sửa hồ sơ recruiter
            if (role == "Recruiter")
                return RedirectToAction(nameof(EditProfile));

            // Candidate / others → trang profile cũ
            var userId = Request.Cookies["UserId"];
            return RedirectToAction(nameof(PersonalProfile), new { userId });
        }

        // ── Stub: ChangePassword ───────────────────────────────────────────────
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Đổi mật khẩu";
            return View();
>>>>>>> Stashed changes
        }
    }
}
