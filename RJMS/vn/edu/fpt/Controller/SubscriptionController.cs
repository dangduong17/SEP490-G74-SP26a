using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentService _paymentService;

        public SubscriptionController(ISubscriptionService subscriptionService, IPaymentService paymentService)
        {
            _subscriptionService = subscriptionService;
            _paymentService = paymentService;
        }

        // ── Subscription Plans Page for Recruiter (GET /Subscription or /Subscription/Index) ──
        [HttpGet]
        public async Task<IActionResult> Index()
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
            
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (int.TryParse(userIdStr, out int userId))
            {
                var activeSubscription = await _paymentService.GetActiveSubscriptionByUserIdAsync(userId);
                ViewBag.CurrentPlanId = activeSubscription?.PlanId;
            }

            // Get active subscription plans with features
            var plans = await _paymentService.GetActiveSubscriptionPlansAsync();
            
            ViewData["Title"] = "Chọn gói dịch vụ";
            return View(plans);
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

            var model = await _subscriptionService.GetPlanListAsync(keyword, status, type, page, pageSize);
            ViewData["Title"] = "Quản lý gói đăng ký";
            return View(model);
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

            await _subscriptionService.CreatePlanAsync(model);
            TempData["Success"] = "Tạo gói đăng ký thành công.";

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Edit (GET) ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditPlan(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;
            ViewData["Title"] = "Chỉnh sửa gói đăng ký";

            var model = await _subscriptionService.GetPlanForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
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

            await _subscriptionService.UpdatePlanAsync(model);
            TempData["Success"] = "Cập nhật gói đăng ký thành công.";

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Toggle status (POST) ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            await _subscriptionService.TogglePlanStatusAsync(id);
            TempData["Success"] = "Đã thay đổi trạng thái gói.";
            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Delete (POST) ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlan(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            bool result = await _subscriptionService.DeletePlanAsync(id);
            
            if (result)
                TempData["Success"] = "Đã xóa gói đăng ký.";
            else
                TempData["Error"] = "Không thể xóa gói đang có người dùng hoặc không tồn tại.";

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Detail JSON (AJAX) ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PlanDetail(int id)
        {
            if (RequireAdmin() is { } redirect) return redirect;

            var detail = await _subscriptionService.GetPlanDetailAsync(id);
            if (detail == null) return NotFound();
            return Json(detail);
        }
    }
}
