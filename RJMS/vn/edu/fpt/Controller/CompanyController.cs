using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        // GET: /Company/Detail/{id}
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            // Try to get current user ID from cookie
            int? currentUserId = null;
            if (int.TryParse(HttpContext.Request.Cookies["UserId"], out int uid))
            {
                currentUserId = uid;
            }

            var model = await _companyService.GetCompanyDetailsAsync(id, currentUserId);
            if (model == null)
            {
                TempData["ErrorToast"] = "Không tìm thấy thông tin công ty.";
                return RedirectToAction("Index", "Job");
            }

            ViewData["Title"] = model.Name + " - Thông tin công ty";
            return View(model);
        }

        // POST: /Company/Follow/{id}
        [HttpPost]
        public async Task<IActionResult> Follow(int id)
        {
            if (!int.TryParse(HttpContext.Request.Cookies["UserId"], out int uid))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để theo dõi công ty." });
            }

            var success = await _companyService.FollowCompanyAsync(id, uid);
            return Json(new { success });
        }

        // POST: /Company/Unfollow/{id}
        [HttpPost]
        public async Task<IActionResult> Unfollow(int id)
        {
            if (!int.TryParse(HttpContext.Request.Cookies["UserId"], out int uid))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var success = await _companyService.UnfollowCompanyAsync(id, uid);
            return Json(new { success });
        }
    }
}
