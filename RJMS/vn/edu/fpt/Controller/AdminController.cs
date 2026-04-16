using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IJobCategoryService _jobCategoryService;
        private readonly ICVService _cvService;

        public AdminController(IAdminService adminService, IJobCategoryService jobCategoryService, ICVService cvService)
        {
            _adminService = adminService;
            _jobCategoryService = jobCategoryService;
            _cvService = cvService;
        }

        private IActionResult? RequireAdminRole()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Admin")
            {
                TempData["WarningToast"] = "Vui lòng đăng nhập với quyền Quản trị viên.";
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }


        public async Task<IActionResult> Index()
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            var model = await _adminService.GetDashboardAsync();
            return View("AdminDashboard", model);
        }

        public async Task<IActionResult> UserList(string? keyword, string? role, string? status, int page = 1, int pageSize = 10)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            var model = await _adminService.GetUserListAsync(keyword, role, status, page, pageSize);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo tài khoản quản trị";
            return View(new AdminCreateAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(AdminCreateAdminViewModel model)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo tài khoản quản trị";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateAdminAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Tạo tài khoản admin thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public IActionResult CreateManager()
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo tài khoản quản lý";
            return View(new AdminCreateManagerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManager(AdminCreateManagerViewModel model)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo tài khoản quản lý";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateManagerAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Tạo tài khoản management thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public IActionResult CreateCandidate()
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo ứng viên";
            return View(new AdminCreateCandidateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCandidate(AdminCreateCandidateViewModel model)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo ứng viên";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateCandidateAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Tạo ứng viên thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public IActionResult CreateRecruiter()
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo nhà tuyển dụng";
            return View(new AdminCreateRecruiterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRecruiter(AdminCreateRecruiterViewModel model)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo nhà tuyển dụng";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateRecruiterAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Tạo nhà tuyển dụng thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            var model = await _adminService.GetUpdateUserAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = "Cập nhật người dùng";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AdminUpdateUserViewModel model)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            ViewData["Title"] = "Cập nhật người dùng";
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.UpdateUserAsync(model);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();

                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Cập nhật người dùng thành công.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (RequireAdminRole() is { } redirect) return redirect;
            var result = await _adminService.SoftDeleteUserAsync(id);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();
                TempData["ErrorToast"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(UserList));
            }

            TempData["SuccessToast"] = "Đã chuyển trạng thái người dùng sang ngưng hoạt động.";
            return RedirectToAction(nameof(UserList));
        }

        // ========== PRIVATE HELPERS ==========

        private void AddErrorsToModelState(ServiceResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Key ?? string.Empty, error.Message);
            }
        }

        // ─────────────────────────────────────────────────────────
        // CV TEMPLATES
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CvTemplates()
        {
            if (RequireAdminRole() is { } r) return r;
            var templates = await _cvService.GetAllTemplatesAsync();
            return View(templates);
        }

        [HttpGet]
        public async Task<IActionResult> CvTemplateCreate()
        {
            if (RequireAdminRole() is { } r) return r;
            ViewBag.Categories = await _cvService.GetAllCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CvTemplateCreate(string name, int? categoryId, string? configJson)
        {
            if (RequireAdminRole() is { } r) return r;
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorToast"] = "Tên Template không được để trống.";
                return View();
            }
            var dto = new RJMS.vn.edu.fpt.Models.DTOs.CvTemplateCreateDTO
            {
                Name = name,
                CategoryId = categoryId,
                ConfigJson = configJson
            };
            var (success, message) = await _cvService.CreateTemplateAsync(dto);
            if (success)
            {
                TempData["SuccessToast"] = message;
                return RedirectToAction(nameof(CvTemplates));
            }
            TempData["ErrorToast"] = message;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CvTemplateEdit(int id)
        {
            if (RequireAdminRole() is { } r) return r;
            var tpl = await _cvService.GetTemplateByIdAsync(id);
            if (tpl == null) { TempData["ErrorToast"] = "Không tìm thấy template."; return RedirectToAction(nameof(CvTemplates)); }
            ViewBag.Categories = await _cvService.GetAllCategoriesAsync();
            return View(tpl);
        }

        [HttpPost]
        public async Task<IActionResult> CvTemplateEdit(int id, string name, int? categoryId, string? configJson, bool isActive)
        {
            if (RequireAdminRole() is { } r) return r;
            var dto = new RJMS.vn.edu.fpt.Models.DTOs.CvTemplateEditDTO
            {
                Id = id,
                Name = name,
                CategoryId = categoryId,
                ConfigJson = configJson,
                IsActive = isActive
            };
            var (success, message) = await _cvService.UpdateTemplateAsync(dto);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            return RedirectToAction(nameof(CvTemplates));
        }

        [HttpPost]
        public async Task<IActionResult> CvTemplateToggle(int id)
        {
            if (RequireAdminRole() is { } r) return r;
            var (_, message) = await _cvService.ToggleTemplateActiveAsync(id);
            TempData["SuccessToast"] = message;
            return RedirectToAction(nameof(CvTemplates));
        }

        [HttpPost]
        public async Task<IActionResult> CvTemplateDelete(int id)
        {
            if (RequireAdminRole() is { } r) return r;
            var (_, message) = await _cvService.DeleteTemplateAsync(id);
            TempData["SuccessToast"] = message;
            return RedirectToAction(nameof(CvTemplates));
        }

        // ─────────────────────────────────────────────────────────
        // TEMPLATE CATEGORIES
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TemplateCategories()
        {
            if (RequireAdminRole() is { } r) return r;
            var categories = await _cvService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TemplateCategoryCreate(RJMS.vn.edu.fpt.Models.DTOs.TemplateCategoryFormDTO dto)
        {
            if (RequireAdminRole() is { } r) return r;
            if (string.IsNullOrWhiteSpace(dto.Name)) {
                TempData["ErrorToast"] = "Tên danh mục không được trống.";
                return RedirectToAction(nameof(TemplateCategories));
            }

            var (success, message) = await _cvService.CreateCategoryAsync(dto);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            return RedirectToAction(nameof(TemplateCategories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TemplateCategoryEdit(RJMS.vn.edu.fpt.Models.DTOs.TemplateCategoryFormDTO dto)
        {
            if (RequireAdminRole() is { } r) return r;
            if (string.IsNullOrWhiteSpace(dto.Name)) {
                TempData["ErrorToast"] = "Tên danh mục không được trống.";
                return RedirectToAction(nameof(TemplateCategories));
            }

            var (success, message) = await _cvService.UpdateCategoryAsync(dto);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            return RedirectToAction(nameof(TemplateCategories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TemplateCategoryDelete(int id)
        {
            if (RequireAdminRole() is { } r) return r;
            var (success, message) = await _cvService.DeleteCategoryAsync(id);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            return RedirectToAction(nameof(TemplateCategories));
        }
    }
}
