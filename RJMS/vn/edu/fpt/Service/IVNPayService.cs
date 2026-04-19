namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(int subscriptionId, int paymentId, decimal amount, string orderInfo, string ipAddress);
        bool ValidateSignature(IQueryCollection queryCollection, string inputHash);
        (bool Success, string Message, string TransactionId) ProcessPaymentCallback(IQueryCollection queryCollection, string? rawQueryString = null);
    }
}
