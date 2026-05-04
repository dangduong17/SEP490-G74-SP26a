using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class ManagerController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IManagerDashboardService _managerDashboardService;
        private readonly IJobCategoryService _jobCategoryService;
        private readonly IWebSliderService _sliderService;
        private readonly ICloudinaryService _cloudinaryService;

        public ManagerController(
            IAdminService adminService,
            IManagerDashboardService managerDashboardService,
            IJobCategoryService jobCategoryService,
            IWebSliderService sliderService,
            ICloudinaryService cloudinaryService)
        {
            _adminService = adminService;
            _managerDashboardService = managerDashboardService;
            _jobCategoryService = jobCategoryService;
            _sliderService = sliderService;
            _cloudinaryService = cloudinaryService;
        }

        private IActionResult? RequireManagerRole()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Manager")
            {
                TempData["WarningToast"] = "Vui lòng đăng nhập với quyền Quản lý nội dung.";
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _managerDashboardService.GetDashboardAsync();
            return View("ManagerDashboard", model);
        }

        [HttpGet]
        public async Task<IActionResult> DashboardData(string from, string to)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
                return BadRequest("Invalid date range");
            if (fromDate > toDate) return BadRequest("from > to");
            var data = await _managerDashboardService.GetDashboardRangeAsync(fromDate, toDate);
            return Json(data);
        }

// ========== SKILLS MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> SkillList(string? keyword, string? category, int page = 1, int pageSize = 20)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _adminService.GetSkillListAsync(keyword, category, page, pageSize);
            ViewData["Title"] = "Quản lý kỹ năng";
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateSkill()
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm kỹ năng mới";
            return View(new AdminCreateSkillViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSkill(AdminCreateSkillViewModel model)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _adminService.GetSkillForEditAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = "Chỉnh sửa kỹ năng";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(AdminUpdateSkillViewModel model)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _jobCategoryService.GetCategoriesAsync(keyword, filterLevel, page, pageSize);
            ViewData["Title"] = "Danh mục nghề nghiệp";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateJobCategory(int level = 1)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm danh mục nghề nghiệp";
            
            var parents = await _jobCategoryService.GetPossibleParentsAsync(level);
            ViewBag.PossibleParents = parents;
            
            return View(new CreateJobCategoryModel { Level = level });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJobCategory(CreateJobCategoryModel model)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _adminService.GetCompanyListAsync(keyword, industry, verificationStatus, page, pageSize);
            ViewData["Title"] = "Quản lý công ty";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CompanyDetail(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _adminService.GetCompanyDetailAsync(id);
            if (model == null) return NotFound();

            ViewData["Title"] = $"Chi tiết công ty - {model.Name}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCompany(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
            
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
            if (RequireManagerRole() is { } redirect) return redirect;
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

        // ========== SLIDER MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> SliderList(string? keyword, string? statusFilter, int page = 1, int pageSize = 15)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _sliderService.GetListAsync(keyword, statusFilter, page, pageSize);
            ViewData["Title"] = "Quản lý Slider";
            return View(model);
        }

        [HttpGet]
        public IActionResult SliderCreate()
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm Slider mới";
            return View(new WebSliderFormViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SliderCreate(WebSliderFormViewModel model)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Thêm Slider mới";

            if (model.ImageFile == null)
                ModelState.AddModelError("ImageFile", "Vui lòng chọn ảnh.");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _sliderService.CreateAsync(model);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            if (success) return RedirectToAction(nameof(SliderList));
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SliderEdit(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            var model = await _sliderService.GetForEditAsync(id);
            if (model == null) return NotFound();
            ViewData["Title"] = "Chỉnh sửa Slider";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SliderEdit(WebSliderFormViewModel model)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Chỉnh sửa Slider";

            // Remove ImageFile validation — it's optional on edit
            ModelState.Remove("ImageFile");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _sliderService.UpdateAsync(model);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            if (success) return RedirectToAction(nameof(SliderList));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SliderDelete(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            var (success, message) = await _sliderService.DeleteAsync(id);
            TempData[success ? "SuccessToast" : "ErrorToast"] = message;
            return RedirectToAction(nameof(SliderList));
        }
    }
}
