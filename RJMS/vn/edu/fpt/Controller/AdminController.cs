using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IJobCategoryService _jobCategoryService;

        public AdminController(IAdminService adminService, IJobCategoryService jobCategoryService)
        {
            _adminService = adminService;
            _jobCategoryService = jobCategoryService;
        }

        private IActionResult? RequireAdmin()
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

            TempData["SuccessToast"] = "Tạo tài khoản admin thành công.";
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

            TempData["SuccessToast"] = "Tạo ứng viên thành công.";
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

            TempData["SuccessToast"] = "Tạo nhà tuyển dụng thành công.";
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

            TempData["SuccessToast"] = "Cập nhật người dùng thành công.";
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
                TempData["ErrorToast"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(UserList));
            }

            TempData["SuccessToast"] = "Đã chuyển trạng thái người dùng sang ngưng hoạt động.";
            return RedirectToAction(nameof(UserList));
        }

        // ========== SKILLS MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> SkillList(string? keyword, string? category, int page = 1, int pageSize = 20)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetSkillListAsync(keyword, category, page, pageSize);
            ViewData["Title"] = "Quản lý kỹ năng";
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateSkill()
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm kỹ năng mới";
            return View(new AdminCreateSkillViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSkill(AdminCreateSkillViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm kỹ năng mới";
            
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateSkillAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Thêm kỹ năng thành công.";
            return RedirectToAction(nameof(SkillList));
        }

        [HttpGet]
        public async Task<IActionResult> EditSkill(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetSkillForEditAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = "Chỉnh sửa kỹ năng";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(AdminUpdateSkillViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Chỉnh sửa kỹ năng";
            
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.UpdateSkillAsync(model);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();
                AddErrorsToModelState(result);
                return View(model);
            }

            TempData["SuccessToast"] = "Cập nhật kỹ năng thành công.";
            return RedirectToAction(nameof(SkillList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var result = await _adminService.DeleteSkillAsync(id);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();
                TempData["ErrorToast"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(SkillList));
            }

            TempData["SuccessToast"] = "Đã xóa kỹ năng.";
            return RedirectToAction(nameof(SkillList));
        }

        // ========== JOB CATEGORIES MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> JobCategoryList(string? keyword, int? filterLevel, int page = 1, int pageSize = 20)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _jobCategoryService.GetCategoriesAsync(keyword, filterLevel, page, pageSize);
            ViewData["Title"] = "Danh mục nghề nghiệp";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateJobCategory(int level = 1)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm danh mục nghề nghiệp";
            
            var parents = await _jobCategoryService.GetPossibleParentsAsync(level);
            ViewBag.PossibleParents = parents;
            
            return View(new CreateJobCategoryModel { Level = level });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJobCategory(CreateJobCategoryModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm danh mục nghề nghiệp";
            
            if (!ModelState.IsValid) 
            {
                ViewBag.PossibleParents = await _jobCategoryService.GetPossibleParentsAsync(model.Level);
                return View(model);
            }

            var result = await _jobCategoryService.CreateCategoryAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                ViewBag.PossibleParents = await _jobCategoryService.GetPossibleParentsAsync(model.Level);
                return View(model);
            }

            TempData["SuccessToast"] = "Thêm danh mục thành công.";
            return RedirectToAction(nameof(JobCategoryList));
        }

        [HttpGet]
        public async Task<IActionResult> EditJobCategory(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var cat = await _jobCategoryService.GetCategoryByIdAsync(id);
            if (cat == null) return NotFound();

            var model = new UpdateJobCategoryModel
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description,
                ParentId = cat.ParentId,
                Level = cat.Level,
                Slug = cat.Slug
            };

            ViewBag.PossibleParents = await _jobCategoryService.GetPossibleParentsAsync(model.Level);
            ViewData["Title"] = "Cập nhật danh mục nghề nghiệp";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobCategory(UpdateJobCategoryModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Cập nhật danh mục nghề nghiệp";
            
            if (!ModelState.IsValid) 
            {
                ViewBag.PossibleParents = await _jobCategoryService.GetPossibleParentsAsync(model.Level);
                return View(model);
            }

            var result = await _jobCategoryService.UpdateCategoryAsync(model);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                ViewBag.PossibleParents = await _jobCategoryService.GetPossibleParentsAsync(model.Level);
                return View(model);
            }

            TempData["SuccessToast"] = "Cập nhật danh mục thành công.";
            return RedirectToAction(nameof(JobCategoryList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobCategory(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var result = await _jobCategoryService.DeleteCategoryAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorToast"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(JobCategoryList));
            }

            TempData["SuccessToast"] = "Đã xóa danh mục.";
            return RedirectToAction(nameof(JobCategoryList));
        }

        // ========== COMPANIES MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> CompanyList(string? keyword, string? industry, string? verificationStatus, int page = 1, int pageSize = 10)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetCompanyListAsync(keyword, industry, verificationStatus, page, pageSize);
            ViewData["Title"] = "Quản lý công ty";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CompanyDetail(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetCompanyDetailAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = $"Chi tiết công ty - {model.Name}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCompany(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var result = await _adminService.VerifyCompanyAsync(id);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();
                TempData["ErrorToast"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(CompanyDetail), new { id });
            }

            TempData["SuccessToast"] = "Đã xác minh công ty.";
            return RedirectToAction(nameof(CompanyDetail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnverifyCompany(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var result = await _adminService.UnverifyCompanyAsync(id);
            if (!result.Succeeded)
            {
                if (result.NotFound) return NotFound();
                TempData["ErrorToast"] = result.Errors.FirstOrDefault()?.Message ?? "Thao tác thất bại.";
                return RedirectToAction(nameof(CompanyDetail), new { id });
            }

            TempData["SuccessToast"] = "Đã hủy xác minh công ty.";
            return RedirectToAction(nameof(CompanyDetail), new { id });
        }

        // ========== SUBSCRIPTIONS MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> SubscriptionList(string? keyword, string? status = "ACTIVE", int? planId = null, int page = 1, int pageSize = 10)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            
            var filterStatus = status == "ALL" ? null : status;
            var model = await _adminService.GetSubscriptionListAsync(keyword, filterStatus, planId, page, pageSize);
            
            if (model != null)
            {
                model.Status = status;
            }

            ViewData["Title"] = "Quản lý gói dịch vụ";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SubscriptionDetail(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            var model = await _adminService.GetSubscriptionDetailAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = $"Chi tiết gói dịch vụ - {model.UserName}";
            return View(model);
        }

        // ========== PRIVATE HELPERS ==========

        private void AddErrorsToModelState(ServiceResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Key ?? string.Empty, error.Message);
            }
        }
    }
}
