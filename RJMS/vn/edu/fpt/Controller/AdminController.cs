using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private IActionResult? RequireAdmin()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Admin")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        public async Task<IActionResult> Index()
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetDashboardAsync();
            return View("AdminDashboard", model);
        }

        public async Task<IActionResult> UserList(string? keyword, string? role, string? status, int page = 1, int pageSize = 10)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetUserListAsync(keyword, role, status, page, pageSize);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo tài khoản quản trị";
            return View(new AdminCreateAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(AdminCreateAdminViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo tài khoản quản trị";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateAdminAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["Success"] = "Tạo tài khoản admin thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public IActionResult CreateCandidate()
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo ứng viên";
            return View(new AdminCreateCandidateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCandidate(AdminCreateCandidateViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo ứng viên";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateCandidateAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["Success"] = "Tạo ứng viên thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public IActionResult CreateRecruiter()
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo nhà tuyển dụng";
            return View(new AdminCreateRecruiterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRecruiter(AdminCreateRecruiterViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo nhà tuyển dụng";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateRecruiterAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["Success"] = "Tạo nhà tuyển dụng thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetUpdateUserAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = "Cập nhật người dùng";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AdminUpdateUserViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Cập nhật người dùng";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.UpdateUserAsync(model);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();

                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["Success"] = "Cập nhật người dùng thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var result = await _adminService.SoftDeleteUserAsync(id);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();
                TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(UserList));
            }

            TempData["Success"] = "Đã chuyển trạng thái người dùng sang ngưng hoạt động.";
            return RedirectToAction(nameof(UserList));
        }

        private void AddErrorsToModelState(ServiceResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Key ?? string.Empty, error.Message);
            }
        }
    }
}
