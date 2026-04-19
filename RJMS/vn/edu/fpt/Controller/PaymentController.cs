using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IVNPayService _vnPayService;

        public PaymentController(IPaymentService paymentService, IVNPayService vnPayService)
        {
            _paymentService = paymentService;
            _vnPayService = vnPayService;
        }

        // GET: /Payment/SubscriptionPlans (redirect to Subscription/Index)
        [HttpGet]
        public IActionResult SubscriptionPlans()
        {
            return RedirectToAction("Index", "Subscription");
        }

        // GET: /Payment/PlanDetail/5
        [HttpGet]
        public async Task<IActionResult> PlanDetail(int id)
        {
            // Check if user is logged in
            var userIdStr = Request.Cookies["UserId"];
            var userRole = Request.Cookies["UserRole"];
            
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["ErrorToast"] = "Vui lòng đăng nhập để xem chi tiết gói";
                return RedirectToAction("Login", "Auth");
            }

            // Only Recruiters can view subscription plan details
            if (userRole != "Recruiter")
            {
                TempData["ErrorToast"] = "Chỉ nhà tuyển dụng mới có thể đăng ký gói dịch vụ";
                return RedirectToAction("Index", "Home");
            }

            var plan = await _paymentService.GetSubscriptionPlanByIdAsync(id);
            if (plan == null)
            {
                TempData["ErrorToast"] = "Gói đăng ký không tồn tại";
                return RedirectToAction(nameof(SubscriptionPlans));
            }

            ViewData["Title"] = $"Chi tiết {plan.Name}";
            return View(plan);
        }

        // POST: /Payment/CreatePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(int planId)
        {
            try
            {
                // Get current user ID from cookie
                var userIdStr = Request.Cookies["UserId"];
                var userRole = Request.Cookies["UserRole"];
                
                if (string.IsNullOrEmpty(userIdStr))
                {
                    TempData["ErrorToast"] = "Vui lòng đăng nhập để tiếp tục";
                    return RedirectToAction("Login", "Auth");
                }

                // Only Recruiters can purchase subscription plans
                if (userRole != "Recruiter")
                {
                    TempData["ErrorToast"] = "Chỉ nhà tuyển dụng mới có thể đăng ký gói dịch vụ";
                    return RedirectToAction("Index", "Home");
                }

                var userId = int.Parse(userIdStr);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // Create subscription, payment and get payment URL
                var (subscriptionId, paymentId, paymentUrl) = await _paymentService.CreatePaymentAsync(userId, planId, ipAddress);

                // Redirect to VNPay
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorToast"] = ex.Message;
                return RedirectToAction(nameof(PlanDetail), new { id = planId });
            }
        }

        // GET: /Payment/VNPayCallback
        [HttpGet]
        public async Task<IActionResult> VNPayCallback()
        {
            try
            {
                // Process callback from VNPay
                var (success, message, transactionId) = _vnPayService.ProcessPaymentCallback(Request.Query, Request.QueryString.Value);

                // Extract paymentId from vnp_TxnRef first (needed for both success and failure)
                var txnRef = Request.Query["vnp_TxnRef"].ToString();
                var paymentId = ExtractPaymentIdFromTxnRef(txnRef);

                if (!success)
                {
                    // Update payment status to FAILED
                    if (paymentId > 0)
                    {
                        await _paymentService.ProcessPaymentFailureAsync(paymentId, transactionId);
                    }
                    TempData["ErrorToast"] = message;
                    return Content($"<script>window.location.href='{Url.Action(nameof(PaymentFailed), "Payment")}';</script>", "text/html");
                }

                if (paymentId == 0)
                {
                    TempData["ErrorToast"] = "Không tìm thấy thông tin thanh toán";
                    return Content($"<script>window.location.href='{Url.Action(nameof(PaymentFailed), "Payment")}';</script>", "text/html");
                }

                // Process payment success: Update Payment, Subscription, Create Period, Invoice, Send Email
                var processed = await _paymentService.ProcessPaymentSuccessAsync(paymentId, transactionId);

                if (processed)
                {
                    TempData["SuccessToast"] = "Thanh toán thành công! Gói dịch vụ đã được kích hoạt.";
                    return Content($"<script>window.location.href='{Url.Action(nameof(PaymentSuccess), "Payment", new { transactionId })}';</script>", "text/html");
                }
                else
                {
                    TempData["ErrorToast"] = "Có lỗi xảy ra khi xử lý thanh toán";
                    return Content($"<script>window.location.href='{Url.Action(nameof(PaymentFailed), "Payment")}';</script>", "text/html");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorToast"] = $"Lỗi: {ex.Message}";
                return Content($"<script>window.location.href='{Url.Action(nameof(PaymentFailed), "Payment")}';</script>", "text/html");
            }
        }

        // GET: /Payment/PaymentSuccess
        [HttpGet]
        public IActionResult PaymentSuccess(string transactionId)
        {
            ViewData["Title"] = "Thanh toán thành công";
            ViewBag.TransactionId = transactionId;
            return View();
        }

        // GET: /Payment/PaymentFailed
        [HttpGet]
        public IActionResult PaymentFailed()
        {
            ViewData["Title"] = "Thanh toán thất bại";
            return View();
        }

        private int ExtractPaymentIdFromTxnRef(string txnRef)
        {
            if (string.IsNullOrEmpty(txnRef)) return 0;

            // Format: {paymentId}_{timestamp}
            var parts = txnRef.Split('_');
            if (parts.Length > 0 && int.TryParse(parts[0], out int paymentId))
            {
                return paymentId;
            }

            return 0;
        }
    }
}
