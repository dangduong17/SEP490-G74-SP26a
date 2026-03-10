using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Service;
using System.Security.Claims;

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
            var plan = await _paymentService.GetSubscriptionPlanByIdAsync(id);
            if (plan == null)
            {
                TempData["Error"] = "Gói đăng ký không tồn tại";
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
                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    TempData["Error"] = "Vui lòng đăng nhập để tiếp tục";
                    return RedirectToAction("Login", "Auth");
                }

                var userId = int.Parse(userIdClaim);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // Create subscription, payment and get payment URL
                var (subscriptionId, paymentId, paymentUrl) = await _paymentService.CreatePaymentAsync(userId, planId, ipAddress);

                // Redirect to VNPay
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
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
                var (success, message, transactionId) = _vnPayService.ProcessPaymentCallback(Request.Query);

                if (!success)
                {
                    TempData["Error"] = message;
                    return RedirectToAction(nameof(PaymentFailed));
                }

                // Extract paymentId from vnp_TxnRef
                var txnRef = Request.Query["vnp_TxnRef"].ToString();
                var paymentId = ExtractPaymentIdFromTxnRef(txnRef);

                if (paymentId == 0)
                {
                    TempData["Error"] = "Không tìm thấy thông tin thanh toán";
                    return RedirectToAction(nameof(PaymentFailed));
                }

                // Process payment success: Update Payment, Subscription, Create Period, Invoice, Send Email
                var processed = await _paymentService.ProcessPaymentSuccessAsync(paymentId, transactionId);

                if (processed)
                {
                    TempData["Success"] = "Thanh toán thành công! Gói dịch vụ đã được kích hoạt.";
                    return RedirectToAction(nameof(PaymentSuccess), new { transactionId });
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán";
                    return RedirectToAction(nameof(PaymentFailed));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(PaymentFailed));
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
