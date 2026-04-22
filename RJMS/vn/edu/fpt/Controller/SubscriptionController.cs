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

        // ── Subscription Plans Page for all authenticated roles (GET /Subscription or /Subscription/Index) ──
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userRole = HttpContext.Request.Cookies["UserRole"];
            
            if (string.IsNullOrWhiteSpace(userRole))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để xem gói dịch vụ";
                return RedirectToAction("Login", "Auth");
            }
            ViewBag.UserRole = userRole;
            ViewBag.CanSubscribe = userRole == "Recruiter";
            
            var userIdStr = HttpContext.Request.Cookies["UserId"];
            if (int.TryParse(userIdStr, out int userId))
            {
                var activeSubscription = await _paymentService.GetActiveSubscriptionByUserIdAsync(userId);
                ViewBag.CurrentPlanId = activeSubscription?.PlanId;
            }

            // Get active subscription plans grouped by base name (Monthly + Yearly variants)
            var groupedPlans = await _subscriptionService.GetGroupedPlansForDisplayAsync();
            
            ViewData["Title"] = "Chọn gói dịch vụ";
            return View(groupedPlans);
        }

        // ── Auth guard ────────────────────────────────────────────────────────
        private IActionResult? RequireManagerRole()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Manager")
            {
                TempData["WarningToast"] = "Hành động yêu cầu quyền Quản lý nội dung.";
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // ── List (GET /Subscription/ManageSubscription) ────────────────────────
        [HttpGet]
        public async Task<IActionResult> ManageSubscription(
            string? keyword, string? status, string? type, int page = 1, int pageSize = 5)
        {
            if (RequireManagerRole() is { } redirect) return redirect;

            var model = await _subscriptionService.GetPlanListAsync(keyword, status, type, page, pageSize);
            ViewData["Title"] = "Quản lý gói đăng ký";
            return View(model);
        }

        // ── Create (GET) ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult CreatePlan()
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo gói đăng ký mới";
            return View(new SubscriptionPlanFormViewModel());
        }

        // ── Create (POST) ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlan(SubscriptionPlanFormViewModel model)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Tạo gói đăng ký mới";

            var selectedCycles = (model.BillingCycles ?? new List<string>())
                .Where(c => c == "Monthly" || c == "Yearly")
                .Distinct()
                .ToList();

            model.BillingCycles = selectedCycles;

            if (selectedCycles.Count == 0)
            {
                ModelState.AddModelError(nameof(model.BillingCycles), "Vui lòng chọn ít nhất một chu kỳ: Hàng tháng hoặc Hàng năm.");
            }

            if (selectedCycles.Contains("Yearly") && (!model.YearlyPrice.HasValue || model.YearlyPrice.Value < 0))
            {
                ModelState.AddModelError(nameof(model.YearlyPrice), "Vui lòng nhập giá năm hợp lệ khi chọn chu kỳ Hàng năm.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var createdIds = await _subscriptionService.CreatePlansForCyclesAsync(model);
            if (createdIds.Count > 1)
                TempData["SuccessToast"] = $"Đã tạo {createdIds.Count} gói đăng ký (Theo tháng & Theo năm).";
            else
                TempData["SuccessToast"] = "Tạo gói đăng ký thành công.";

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Edit (GET) ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditPlan(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;
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
            if (RequireManagerRole() is { } redirect) return redirect;
            ViewData["Title"] = "Chỉnh sửa gói đăng ký";

            if (model.EnableYearly && (!model.YearlyPrice.HasValue || model.YearlyPrice.Value < 0))
            {
                ModelState.AddModelError(nameof(model.YearlyPrice), "Vui lòng nhập giá năm hợp lệ khi bật gói năm.");
            }

            if (!ModelState.IsValid)
                return View(model);

            await _subscriptionService.UpdatePlanAsync(model);
            TempData["SuccessToast"] = "Cập nhật gói đăng ký thành công.";

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Toggle status (POST) ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;

            await _subscriptionService.TogglePlanStatusAsync(id);
            TempData["SuccessToast"] = "Đã thay đổi trạng thái gói.";
            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Delete (POST) ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlan(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;

            bool result = await _subscriptionService.DeletePlanAsync(id);
            
            if (result)
                TempData["SuccessToast"] = "Đã xóa gói đăng ký.";
            else
                TempData["ErrorToast"] = "Không thể xóa gói đang có người dùng hoặc không tồn tại.";

            return RedirectToAction(nameof(ManageSubscription));
        }

        // ── Detail JSON (AJAX) ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PlanDetail(int id)
        {
            if (RequireManagerRole() is { } redirect) return redirect;

            var detail = await _subscriptionService.GetPlanDetailAsync(id);
            if (detail == null) return NotFound();
            return Json(detail);
        }
    }
}
