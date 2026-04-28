using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IPaymentRepository
    {
        Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync();
        Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int planId);
        Task<int> CreateSubscriptionAsync(int userId, int planId);
        Task<int> CreatePaymentAsync(int subscriptionId, decimal amount);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status, string transactionId);
        Task<bool> UpdateSubscriptionStatusAsync(int subscriptionId, string status);
        Task<bool> CreateSubscriptionPeriodAsync(int subscriptionId, int planId, DateTime startDate, DateTime endDate);
        Task<int> CreateInvoiceAsync(Invoice invoice);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId);
        Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        /// <summary>Hủy (Expired) tất cả subscription Active cũ (trừ subscriptionId vừa tạo mới).</summary>
        Task CancelPreviousActiveSubscriptionsAsync(int userId, int? companyId, int newSubscriptionId);
    }
}
