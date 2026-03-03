using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

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
        public async Task<IActionResult> PersonalProfile(Guid? userId)
        {
            var profile = await _profileService.GetPersonalProfileAsync(userId ?? Guid.Empty);

            if (profile == null)
            {
                ModelState.AddModelError(string.Empty, "Profile not found");
                profile = new UserProfileDTO();
            }

            return View("PersonalProfile", profile);
        }
    }
}
