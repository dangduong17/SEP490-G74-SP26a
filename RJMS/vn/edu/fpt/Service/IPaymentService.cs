using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IPaymentService
    {
        Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync();
        Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int planId);
        Task<(int SubscriptionId, int PaymentId, string PaymentUrl)> CreatePaymentAsync(int userId, int planId, string ipAddress);
        Task<bool> ProcessPaymentSuccessAsync(int paymentId, string transactionId);
    }
}
