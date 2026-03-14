using RJMS.vn.edu.fpt.Models;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IVNPayService _vnPayService;
        private readonly IEmailService _emailService;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IVNPayService vnPayService,
            IEmailService emailService)
        {
            _paymentRepo = paymentRepo;
            _vnPayService = vnPayService;
            _emailService = emailService;
        }

        public async Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync()
        {
            return await _paymentRepo.GetActiveSubscriptionPlansAsync();
        }

        public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int planId)
        {
            return await _paymentRepo.GetSubscriptionPlanByIdAsync(planId);
        }

        public async Task<(int SubscriptionId, int PaymentId, string PaymentUrl)> CreatePaymentAsync(
            int userId, int planId, string ipAddress)
        {
            var plan = await _paymentRepo.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                throw new Exception("Gói đăng ký không tồn tại");
            }

            // 1. Create Subscription (status = PENDING)
            var subscriptionId = await _paymentRepo.CreateSubscriptionAsync(userId, planId);

            // 2. Create Payment (status = PENDING)
            var paymentId = await _paymentRepo.CreatePaymentAsync(subscriptionId, plan.Price ?? 0);

            // 3. Generate VNPay payment URL
            var orderInfo = $"Thanh toan goi {plan.Name}";
            var paymentUrl = _vnPayService.CreatePaymentUrl(
                subscriptionId,
                paymentId,
                plan.Price ?? 0,
                orderInfo,
                ipAddress
            );

            return (subscriptionId, paymentId, paymentUrl);
        }

        public async Task<bool> ProcessPaymentSuccessAsync(int paymentId, string transactionId)
        {
            var payment = await _paymentRepo.GetPaymentByIdAsync(paymentId);
            if (payment == null) return false;

            var subscription = await _paymentRepo.GetSubscriptionByIdAsync(payment.SubscriptionId);
            if (subscription == null) return false;

            // 1. Update Payment = SUCCESS
            await _paymentRepo.UpdatePaymentStatusAsync(paymentId, "SUCCESS", transactionId);

            // 2. Update Subscription = ACTIVE
            await _paymentRepo.UpdateSubscriptionStatusAsync(payment.SubscriptionId, "ACTIVE");

            // 3. Create SubscriptionPeriod
            await _paymentRepo.CreateSubscriptionPeriodAsync(
                payment.SubscriptionId,
                subscription.PlanId,
                subscription.StartDate ?? DateTime.UtcNow,
                subscription.EndDate ?? DateTime.UtcNow.AddDays(30)
            );

            // 4. Create Invoice
            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{paymentId:D6}";
            var invoice = new Invoice
            {
                SubscriptionId = payment.SubscriptionId,
                PaymentId = paymentId,
                InvoiceNumber = invoiceNumber,
                Amount = payment.Amount,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = "PAID",
                Description = $"Hóa đơn thanh toán gói {subscription.Plan?.Name}",
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepo.CreateInvoiceAsync(invoice);

            // 5. Send email
            var user = subscription.User;
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailSubject = "Xác nhận thanh toán thành công - RJMS";
                var emailBody = $@"
                    <h2>Xin chào {user.FirstName} {user.LastName},</h2>
                    <p>Cảm ơn bạn đã đăng ký gói dịch vụ tại RJMS!</p>
                    <h3>Thông tin đơn hàng:</h3>
                    <ul>
                        <li><strong>Số hóa đơn:</strong> {invoiceNumber}</li>
                        <li><strong>Gói dịch vụ:</strong> {subscription.Plan?.Name}</li>
                        <li><strong>Số tiền:</strong> {payment.Amount:N0} VND</li>
                        <li><strong>Mã giao dịch:</strong> {transactionId}</li>
                        <li><strong>Thời hạn:</strong> {subscription.StartDate:dd/MM/yyyy} - {subscription.EndDate:dd/MM/yyyy}</li>
                    </ul>
                    <p>Gói dịch vụ của bạn đã được kích hoạt thành công.</p>
                    <p>Trân trọng,<br/>RJMS Team</p>
                ";

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            }

            return true;
        }

        public async Task<bool> ProcessPaymentFailureAsync(int paymentId, string transactionId)
        {
            var payment = await _paymentRepo.GetPaymentByIdAsync(paymentId);
            if (payment == null) return false;

            // 1. Update Payment = FAILED
            await _paymentRepo.UpdatePaymentStatusAsync(paymentId, "FAILED", transactionId);

            // 2. Update Subscription = CANCELLED
            await _paymentRepo.UpdateSubscriptionStatusAsync(payment.SubscriptionId, "CANCELLED");

            return true;
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId)
        {
            return await _paymentRepo.GetActiveSubscriptionByUserIdAsync(userId);
        }
    }
}
