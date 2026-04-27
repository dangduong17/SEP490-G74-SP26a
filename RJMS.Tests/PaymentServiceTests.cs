using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RJMS.vn.edu.fpt.Models;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class PaymentServiceTests
    {
        private Mock<IPaymentRepository> _paymentRepoMock;
        private Mock<IVNPayService> _vnPaySvcMock;
        private Mock<IEmailService> _emailSvcMock;
        private PaymentService _service;

        public PaymentServiceTests()
        {
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _vnPaySvcMock = new Mock<IVNPayService>();
            _emailSvcMock = new Mock<IEmailService>();
            _service = new PaymentService(_paymentRepoMock.Object, _vnPaySvcMock.Object, _emailSvcMock.Object);
        }

        // --- FUNC22: CreatePaymentAsync ---

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "CreatePaymentAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task CreatePayment_UTC01_Success()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(1)).ReturnsAsync(new SubscriptionPlan { Id = 1, Price = 1000, Name = "Vip" });
            _paymentRepoMock.Setup(r => r.CreateSubscriptionAsync(1, 1)).ReturnsAsync(10);
            _paymentRepoMock.Setup(r => r.CreatePaymentAsync(10, 1000)).ReturnsAsync(20);
            _vnPaySvcMock.Setup(v => v.CreatePaymentUrl(10, 20, 1000, It.IsAny<string>(), "1.1.1.1")).Returns("http://vnpay.vn");

            var result = await _service.CreatePaymentAsync(1, 1, "1.1.1.1");
            Assert.Equal(10, result.SubscriptionId);
            Assert.Equal("http://vnpay.vn", result.PaymentUrl);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "CreatePaymentAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task CreatePayment_UTC02_PlanNotFound()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(99)).ReturnsAsync((SubscriptionPlan)null);
            await Assert.ThrowsAsync<Exception>(() => _service.CreatePaymentAsync(1, 99, "1.1.1.1"));
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "CreatePaymentAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task CreatePayment_UTC03_RepoError()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB Fail"));
            await Assert.ThrowsAsync<Exception>(() => _service.CreatePaymentAsync(1, 1, "1.1.1.1"));
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "CreatePaymentAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task CreatePayment_UTC04_ZeroPlanId()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(0)).ReturnsAsync((SubscriptionPlan)null);
            await Assert.ThrowsAsync<Exception>(() => _service.CreatePaymentAsync(1, 0, "1.1.1.1"));
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "CreatePaymentAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task CreatePayment_UTC05_EmptyIP()
        {
             _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(1)).ReturnsAsync(new SubscriptionPlan { Id = 1, Price = 1000 });
             _vnPaySvcMock.Setup(v => v.CreatePaymentUrl(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), "")).Returns("Error");
             var result = await _service.CreatePaymentAsync(1, 1, "");
             Assert.Equal("Error", result.PaymentUrl);
        }

        // --- FUNC23: ProcessPaymentSuccessAsync ---

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentSuccessAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task ProcessSuccess_UTC01_Success()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(new Payment { Id = 1, SubscriptionId = 10 });
            _paymentRepoMock.Setup(r => r.GetSubscriptionByIdAsync(10)).ReturnsAsync(new Subscription { Id = 10, PlanId = 1 });
            var result = await _service.ProcessPaymentSuccessAsync(1, "TXN123");
            Assert.True(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentSuccessAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task ProcessSuccess_UTC02_PaymentNotFound()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(99)).ReturnsAsync((Payment)null);
            var result = await _service.ProcessPaymentSuccessAsync(99, "TXN123");
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentSuccessAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task ProcessSuccess_UTC03_SubscriptionNotFound()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(new Payment { Id = 1, SubscriptionId = 10 });
            _paymentRepoMock.Setup(r => r.GetSubscriptionByIdAsync(10)).ReturnsAsync((Subscription)null);
            var result = await _service.ProcessPaymentSuccessAsync(1, "TXN123");
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentSuccessAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task ProcessSuccess_UTC04_RepoError()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _service.ProcessPaymentSuccessAsync(1, "TXN123"));
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentSuccessAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task ProcessSuccess_UTC05_EmptyTxnId()
        {
             _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(new Payment { Id = 1, SubscriptionId = 10 });
             _paymentRepoMock.Setup(r => r.GetSubscriptionByIdAsync(10)).ReturnsAsync(new Subscription { Id = 10 });
             var result = await _service.ProcessPaymentSuccessAsync(1, "");
             Assert.True(result); // Implementation still proceeds
        }

        // --- FUNC24: ProcessPaymentFailureAsync ---

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentFailureAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task ProcessFailure_UTC01_Success()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(new Payment { Id = 1 });
            var result = await _service.ProcessPaymentFailureAsync(1, "TXN123");
            Assert.True(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentFailureAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task ProcessFailure_UTC02_NotFound()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(99)).ReturnsAsync((Payment)null);
            var result = await _service.ProcessPaymentFailureAsync(99, "TXN123");
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentFailureAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task ProcessFailure_UTC03_RepoError()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(1)).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _service.ProcessPaymentFailureAsync(1, "TXN123"));
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentFailureAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task ProcessFailure_UTC04_ZeroId()
        {
            var result = await _service.ProcessPaymentFailureAsync(0, "TXN123");
            Assert.False(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentFailureAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task ProcessFailure_UTC05_EmptyTxn()
        {
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(new Payment { Id = 1 });
            var result = await _service.ProcessPaymentFailureAsync(1, "");
            Assert.True(result);
        }

        // --- FUNC25: GetActiveSubscriptionPlansAsync ---

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetActiveSubscriptionPlansAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetPlans_UTC01_Success()
        {
            _paymentRepoMock.Setup(r => r.GetActiveSubscriptionPlansAsync()).ReturnsAsync(new List<SubscriptionPlan> { new SubscriptionPlan() });
            var result = await _service.GetActiveSubscriptionPlansAsync();
            Assert.Single(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetActiveSubscriptionPlansAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetPlans_UTC02_RepoError()
        {
            _paymentRepoMock.Setup(r => r.GetActiveSubscriptionPlansAsync()).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _service.GetActiveSubscriptionPlansAsync());
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetActiveSubscriptionPlansAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetPlans_UTC03_Empty()
        {
            _paymentRepoMock.Setup(r => r.GetActiveSubscriptionPlansAsync()).ReturnsAsync(new List<SubscriptionPlan>());
            var result = await _service.GetActiveSubscriptionPlansAsync();
            Assert.Empty(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetActiveSubscriptionPlansAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "A")]
        public async Task GetPlans_UTC04_Multiple()
        {
            _paymentRepoMock.Setup(r => r.GetActiveSubscriptionPlansAsync()).ReturnsAsync(new List<SubscriptionPlan> { new SubscriptionPlan(), new SubscriptionPlan() });
            var result = await _service.GetActiveSubscriptionPlansAsync();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetActiveSubscriptionPlansAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetPlans_UTC05_NullReturn()
        {
             _paymentRepoMock.Setup(r => r.GetActiveSubscriptionPlansAsync()).ReturnsAsync((List<SubscriptionPlan>)null);
             var result = await _service.GetActiveSubscriptionPlansAsync();
             Assert.Null(result);
        }

        // --- FUNC26: GetSubscriptionPlanByIdAsync ---

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetSubscriptionPlanByIdAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetPlanById_UTC01_Success()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(1)).ReturnsAsync(new SubscriptionPlan { Id = 1 });
            var result = await _service.GetSubscriptionPlanByIdAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetSubscriptionPlanByIdAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task GetPlanById_UTC02_NotFound()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(99)).ReturnsAsync((SubscriptionPlan)null);
            var result = await _service.GetSubscriptionPlanByIdAsync(99);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetSubscriptionPlanByIdAsync")]
        [Trait("UTCID", "UTCID03")]
        [Trait("Type", "B")]
        public async Task GetPlanById_UTC03_RepoError()
        {
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Fail"));
            await Assert.ThrowsAsync<Exception>(() => _service.GetSubscriptionPlanByIdAsync(1));
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetSubscriptionPlanByIdAsync")]
        [Trait("UTCID", "UTCID04")]
        [Trait("Type", "B")]
        public async Task GetPlanById_UTC04_ZeroId()
        {
            var result = await _service.GetSubscriptionPlanByIdAsync(0);
            Assert.Null(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetSubscriptionPlanByIdAsync")]
        [Trait("UTCID", "UTCID05")]
        [Trait("Type", "B")]
        public async Task GetPlanById_UTC05_NegativeId()
        {
            var result = await _service.GetSubscriptionPlanByIdAsync(-1);
            Assert.Null(result);
        }
    }
}
