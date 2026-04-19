using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(int subscriptionId, int paymentId, decimal amount, string orderInfo, string ipAddress)
        {
            var tmnCode = _configuration["VNPay:TmnCode"]!;
            var hashSecret = _configuration["VNPay:HashSecret"]!;
            var vnpUrl = _configuration["VNPay:Url"]!;
            var returnUrl = _configuration["VNPay:ReturnUrl"]!;

            var vnPayData = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() }, // VNPay yêu cầu số tiền * 100
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", $"{paymentId}_{DateTime.Now.Ticks}" } // Mã giao dịch unique
            };

            var buildString = new StringBuilder();

            foreach (var kv in vnPayData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    buildString.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            var signString = buildString.ToString();
            if (signString.Length > 0)
            {
                signString = signString.Substring(0, signString.Length - 1);
            }
            if (buildString.Length > 0)
            {
                buildString = buildString.Remove(buildString.Length - 1, 1);
            }

            var vnpSecureHash = HmacSHA512(hashSecret, signString);

            return $"{vnpUrl}?{buildString.ToString()}&vnp_SecureHash={vnpSecureHash}";
        }

        public bool ValidateSignature(IQueryCollection queryCollection, string inputHash)
        {
            var hashSecret = _configuration["VNPay:HashSecret"]!;

            var vnPayData = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var key in queryCollection.Keys)
            {
                if (key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    vnPayData.Add(key, queryCollection[key]!);
                }
            }

            var signString = new StringBuilder();
            foreach (var kv in vnPayData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    signString.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            if (signString.Length > 0) signString.Remove(signString.Length - 1, 1);

            var checkSum = HmacSHA512(hashSecret, signString.ToString());

            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public (bool Success, string Message, string TransactionId) ProcessPaymentCallback(IQueryCollection queryCollection, string? rawQueryString = null)
        {
            var vnpSecureHash = queryCollection["vnp_SecureHash"].ToString();

            var isValid = false;
            if (!string.IsNullOrEmpty(vnpSecureHash))
            {
                isValid = !string.IsNullOrWhiteSpace(rawQueryString)
                    ? ValidateSignatureFromRawQueryString(rawQueryString!, vnpSecureHash)
                    : ValidateSignature(queryCollection, vnpSecureHash);
            }

            if (string.IsNullOrEmpty(vnpSecureHash) || !isValid)
            {
                return (false, "Chữ ký không hợp lệ", string.Empty);
            }

            var responseCode = queryCollection["vnp_ResponseCode"].ToString();
            var transactionId = queryCollection["vnp_TransactionNo"].ToString();
            var txnRef = queryCollection["vnp_TxnRef"].ToString();

            if (responseCode == "00")
            {
                return (true, "Giao dịch thành công", transactionId);
            }
            else
            {
                var errorMessage = GetResponseDescription(responseCode);
                return (false, errorMessage, transactionId);
            }
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private bool ValidateSignatureFromRawQueryString(string rawQueryString, string inputHash)
        {
            var hashSecret = _configuration["VNPay:HashSecret"]!;
            var signString = BuildCanonicalRawQueryString(rawQueryString);
            var checkSum = HmacSHA512(hashSecret, signString);

            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string BuildCanonicalRawQueryString(string rawQueryString)
        {
            if (string.IsNullOrWhiteSpace(rawQueryString))
            {
                return string.Empty;
            }

            var query = rawQueryString.TrimStart('?');
            var items = new List<KeyValuePair<string, string>>();

            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var equalsIndex = part.IndexOf('=');
                var key = equalsIndex >= 0 ? part[..equalsIndex] : part;
                var value = equalsIndex >= 0 ? part[(equalsIndex + 1)..] : string.Empty;

                if (key == "vnp_SecureHash" || key == "vnp_SecureHashType")
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    items.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            var sorted = items.OrderBy(item => item.Key, StringComparer.Ordinal);
            var builder = new StringBuilder();

            foreach (var item in sorted)
            {
                builder.Append(item.Key).Append('=').Append(item.Value).Append('&');
            }

            if (builder.Length > 0)
            {
                builder.Length--;
            }

            return builder.ToString();
        }

        private string GetResponseDescription(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa",
                "13" => "Giao dịch không thành công do: Quý khách nhập sai mật khẩu xác thực giao dịch (OTP)",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định",
                _ => "Giao dịch không thành công"
            };
        }
    }
}
