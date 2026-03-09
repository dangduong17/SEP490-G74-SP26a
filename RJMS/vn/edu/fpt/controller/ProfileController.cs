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

        // ── Helpers ───────────────────────────────────────────────────────────
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

        private IActionResult? RequireAdmin()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Admin")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        private IActionResult? RequireCandidate()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Candidate")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ── Candidate: Xem hồ sơ ─────────────────────────────────────────────
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

        // ── Đổi mật khẩu (GET) ───────────────────────────────────────────────
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (string.IsNullOrWhiteSpace(userIdStr))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để đổi mật khẩu";
                return RedirectToAction("Login", "Auth");
            }

            ViewData["Title"] = "Đổi mật khẩu";
            return View(new ChangePasswordViewModel());
        }

        // ── Đổi mật khẩu (POST) ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để đổi mật khẩu";
                return RedirectToAction("Login", "Auth");
            }

            ViewData["Title"] = "Đổi mật khẩu";

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _profileService.ChangePasswordAsync(
                userId, model.CurrentPassword, model.NewPassword);

            if (success)
            {
                TempData["SuccessToast"] = message;
                return RedirectToAction("ChangePassword");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // ── Recruiter: Chỉnh sửa hồ sơ (GET) ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = await _profileService.GetRecruiterProfileAsync(userId.Value)
                        ?? new RecruiterProfileUpdateViewModel();

            ViewData["Title"] = "Chỉnh sửa hồ sơ nhà tuyển dụng";
            return View("~/Views/Recruiter/EditProfile.cshtml", model);
        }

        // ── Recruiter: Chỉnh sửa hồ sơ (POST) ───────────────────────────────
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

            // Đồng bộ cookie UserName
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

        // ── Redirect thông minh theo role ─────────────────────────────────────
        [HttpGet]
        public IActionResult Info()
        {
            var role = Request.Cookies["UserRole"];
            var userId = GetCurrentUserId();

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (role == "Recruiter")
                return RedirectToAction(nameof(EditProfileNew));
            if (role == "Admin")
                return RedirectToAction(nameof(EditAdminProfile));
            if (role == "Candidate")
                return RedirectToAction(nameof(EditCandidateProfile));

            return RedirectToAction("Login", "Auth");
        }

        // ====== CANDIDATE EDIT PROFILE ======
        [HttpGet]
        public async Task<IActionResult> EditCandidateProfile()
        {
            if (RequireCandidate() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = await _profileService.GetCandidateProfileForEditAsync(userId.Value)
                        ?? new CandidateEditProfileViewModel();

            ViewData["Title"] = "Chỉnh sửa hồ sơ cá nhân";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCandidateProfile(CandidateEditProfileViewModel model)
        {
            if (RequireCandidate() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Chỉnh sửa hồ sơ cá nhân";

            if (!ModelState.IsValid)
                return View(model);

            var success = await _profileService.UpdateCandidateProfileAsync(userId.Value, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Cập nhật thất bại. Vui lòng thử lại.");
                return View(model);
            }

            // Đồng bộ cookie
            var fullName = $"{model.FirstName} {model.LastName}".Trim();
            Response.Cookies.Append("UserName", fullName, new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            TempData["SuccessToast"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("CandidateDashboard", "Dashboard");
        }

        // ====== RECRUITER NEW EDIT PROFILE ======
        [HttpGet]
        public async Task<IActionResult> EditProfileNew()
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = await _profileService.GetRecruiterProfileForEditAsync(userId.Value)
                        ?? new RecruiterEditProfileViewModel();

            ViewData["Title"] = "Chỉnh sửa hồ sơ nhà tuyển dụng";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileNew(RecruiterEditProfileViewModel model)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Chỉnh sửa hồ sơ nhà tuyển dụng";

            if (!ModelState.IsValid)
                return View(model);

            var success = await _profileService.UpdateRecruiterProfileNewAsync(userId.Value, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Cập nhật thất bại. Vui lòng thử lại.");
                return View(model);
            }

            // Đồng bộ cookie
            var fullName = $"{model.FirstName} {model.LastName}".Trim();
            Response.Cookies.Append("UserName", fullName, new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            TempData["SuccessToast"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("RecruiterDashboard", "Recruiter");
        }

        // ====== ADMIN EDIT PROFILE ======
        [HttpGet]
        public async Task<IActionResult> EditAdminProfile()
        {
            if (RequireAdmin() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = await _profileService.GetAdminProfileForEditAsync(userId.Value)
                        ?? new AdminEditProfileViewModel();

            ViewData["Title"] = "Chỉnh sửa hồ sơ quản trị viên";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdminProfile(AdminEditProfileViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Chỉnh sửa hồ sơ quản trị viên";

            if (!ModelState.IsValid)
                return View(model);

            var success = await _profileService.UpdateAdminProfileAsync(userId.Value, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Cập nhật thất bại. Vui lòng thử lại.");
                return View(model);
            }

            // Đồng bộ cookie
            var fullName = $"{model.FirstName} {model.LastName}".Trim();
            Response.Cookies.Append("UserName", fullName, new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            TempData["SuccessToast"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Index", "Admin");
        }
    }
}
