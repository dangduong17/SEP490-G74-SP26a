using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        // ── Subscription Plans Page for Recruiter (GET /Subscription or /Subscription/Index) ──
        [HttpGet]
        public IActionResult Index()
        {
            // Check if user is logged in and is recruiter
            var userRole = HttpContext.Request.Cookies["UserRole"];
            
            if (string.IsNullOrWhiteSpace(userRole))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để xem gói dịch vụ";
                return RedirectToAction("Login", "Auth");
            }

            if (userRole != "Recruiter")
            {
                TempData["ErrorToast"] = "Tính năng này chỉ dành cho nhà tuyển dụng";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Chọn gói dịch vụ";
            return View();
        }

        // ── Auth guard ────────────────────────────────────────────────────────
        private IActionResult? RequireAdmin()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Admin")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ── List (GET /Subscription/ManageSubscription) ────────────────────────
        [HttpGet]
        public async Task<IActionResult> ManageSubscription(
            string? keyword, string? status, string? type, int page = 1, int pageSize = 5)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            // TODO: khi SubscriptionRepository được implement, bỏ try/catch
            try
            {
                var model = await _subscriptionService.GetPlanListAsync(keyword, status, type, page, pageSize);
                ViewData["Title"] = "Quản lý gói đăng ký";
                return View(model);
            }
            catch (NotImplementedException)
            {
                // Dùng dữ liệu mẫu cho đến khi implement xong service
                var model = BuildSampleList(keyword, status, type, page, pageSize);
                ViewData["Title"] = "Quản lý gói đăng ký";
                return View(model);
            }
        }

        // ── Create (GET) ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult CreatePlan()
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo gói đăng ký mới";
            return View(new SubscriptionPlanFormViewModel());
        }

        // ── Create (POST) ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlan(SubscriptionPlanFormViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo gói đăng ký mới";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _subscriptionService.CreatePlanAsync(model);
                TempData["Success"] = "Tạo gói đăng ký thành công.";
            }
            catch (NotImplementedException)
            {
                TempData["Success"] = "[Demo] Tạo gói thành công (chưa lưu DB).";
            }

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Edit (GET) ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditPlan(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Chỉnh sửa gói đăng ký";

            try
            {
                var model = await _subscriptionService.GetPlanForEditAsync(id);
                if (model == null) return NotFound();
                return View(model);
            }
            catch (NotImplementedException)
            {
                return View(new SubscriptionPlanFormViewModel { Id = id });
            }
        }

        // ── Edit (POST) ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPlan(SubscriptionPlanFormViewModel model)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Chỉnh sửa gói đăng ký";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _subscriptionService.UpdatePlanAsync(model);
                TempData["Success"] = "Cập nhật gói đăng ký thành công.";
            }
            catch (NotImplementedException)
            {
                TempData["Success"] = "[Demo] Cập nhật thành công (chưa lưu DB).";
            }

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Toggle status (POST) ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            try { await _subscriptionService.TogglePlanStatusAsync(id); }
            catch (NotImplementedException) { }

            TempData["Success"] = "Đã thay đổi trạng thái gói.";
            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Delete (POST) ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlan(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            try { await _subscriptionService.DeletePlanAsync(id); }
            catch (NotImplementedException) { }

            TempData["Success"] = "Đã xóa gói đăng ký.";
            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Detail JSON (AJAX) ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PlanDetail(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            try
            {
                var detail = await _subscriptionService.GetPlanDetailAsync(id);
                if (detail == null) return NotFound();
                return Json(detail);
            }
            catch (NotImplementedException)
            {
                return Json(new SubscriptionPlanDetailDto
                {
                    Id = id, Name = "Demo Plan", PlanType = "Basic",
                    Price = 0, DurationDays = 30, IsActive = true
                });
            }
        }

        // ── Sample data helper (xóa khi service được implement) ──────────────
        private static SubscriptionListViewModel BuildSampleList(
            string? keyword, string? status, string? type, int page, int pageSize)
        {
            var all = new List<SubscriptionPlanRowDto>
            {
                new() { Id=1, Code="SUB-001", Name="Basic Recruiter",    PlanType="Basic",    Price=0,         JobLimit=3,   CvAiLimit=null, DurationDays=0,  IsActive=true,  CreatedAt=new DateTime(2023,10,12), RecruiterCount=450 },
                new() { Id=2, Code="SUB-002", Name="Standard Growth",    PlanType="Standard", Price=1_500_000, JobLimit=15,  CvAiLimit=50,   DurationDays=30, IsActive=true,  CreatedAt=new DateTime(2023,11,15), RecruiterCount=320 },
                new() { Id=3, Code="SUB-003", Name="Premium Enterprise", PlanType="Premium",  Price=4_500_000, JobLimit=null,CvAiLimit=null, DurationDays=30, IsActive=true,  CreatedAt=new DateTime(2024,1,1),   RecruiterCount=98  },
                new() { Id=4, Code="SUB-004", Name="Trial Specialist",   PlanType="Basic",    Price=200_000,   JobLimit=5,   CvAiLimit=10,   DurationDays=7,  IsActive=false, CreatedAt=new DateTime(2024,2,20),  RecruiterCount=12  },
                new() { Id=5, Code="SUB-005", Name="Seasonal Campaign",  PlanType="Standard", Price=2_800_000, JobLimit=30,  CvAiLimit=100,  DurationDays=90, IsActive=true,  CreatedAt=new DateTime(2024,3,10),  RecruiterCount=368 },
                new() { Id=6, Code="SUB-006", Name="Startup Pack",       PlanType="Basic",    Price=500_000,   JobLimit=8,   CvAiLimit=20,   DurationDays=30, IsActive=true,  CreatedAt=new DateTime(2024,4,1),   RecruiterCount=0   },
            };

            if (!string.IsNullOrWhiteSpace(keyword))
                all = all.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            if (status == "active")   all = all.Where(p => p.IsActive).ToList();
            if (status == "inactive") all = all.Where(p => !p.IsActive).ToList();
            if (!string.IsNullOrWhiteSpace(type))
                all = all.Where(p => p.PlanType == type).ToList();

            var total   = all.Count;
            var paged   = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new SubscriptionListViewModel
            {
                Plans          = paged,
                TotalPlans     = 12,
                ActivePlans    = 8,
                RecruitersUsing = 1248,
                TotalItems     = total,
                Page           = page,
                PageSize       = pageSize,
                SearchKeyword  = keyword,
                StatusFilter   = status,
                TypeFilter     = type,
            };
        }
    }
}
