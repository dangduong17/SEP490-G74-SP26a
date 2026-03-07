using Microsoft.AspNetCore.Mvc;
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
        }
    }
}
