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
        private PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _vnPaySvcMock = new Mock<IVNPayService>();
            _emailSvcMock = new Mock<IEmailService>();
            _paymentService = new PaymentService(
                _paymentRepoMock.Object,
                _vnPaySvcMock.Object,
                _emailSvcMock.Object
            );
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "CreatePaymentAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task CreatePaymentAsync_ValidPlan_ReturnsUrl()
        {
            // Arrange
            var plan = new SubscriptionPlan
            {
                Id = 1,
                Name = "Pro",
                Price = 100000,
            };
            _paymentRepoMock.Setup(r => r.GetSubscriptionPlanByIdAsync(1)).ReturnsAsync(plan);
            _paymentRepoMock.Setup(r => r.CreateSubscriptionAsync(1, 1)).ReturnsAsync(10);
            _paymentRepoMock.Setup(r => r.CreatePaymentAsync(10, 100000)).ReturnsAsync(100);
            _vnPaySvcMock
                .Setup(v => v.CreatePaymentUrl(10, 100, 100000, It.IsAny<string>(), "127.0.0.1"))
                .Returns("http://vnpay");

            // Act
            var result = await _paymentService.CreatePaymentAsync(1, 1, "127.0.0.1");

            // Assert
            Assert.Equal(10, result.SubscriptionId);
            Assert.Equal("http://vnpay", result.PaymentUrl);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentSuccessAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task ProcessPaymentSuccessAsync_ValidPayment_UpdatesStatus()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 100,
                SubscriptionId = 10,
                Amount = 100000,
            };
            var sub = new Subscription
            {
                Id = 10,
                PlanId = 1,
                User = new User { Email = "test@test.com" },
            };
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(100)).ReturnsAsync(payment);
            _paymentRepoMock.Setup(r => r.GetSubscriptionByIdAsync(10)).ReturnsAsync(sub);

            // Act
            var result = await _paymentService.ProcessPaymentSuccessAsync(100, "TRANS123");

            // Assert
            Assert.True(result);
            _paymentRepoMock.Verify(
                r => r.UpdatePaymentStatusAsync(100, "SUCCESS", "TRANS123"),
                Times.Once
            );
            _paymentRepoMock.Verify(r => r.UpdateSubscriptionStatusAsync(10, "ACTIVE"), Times.Once);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "ProcessPaymentFailureAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task ProcessPaymentFailureAsync_UpdatesStatusToFailed()
        {
            // Arrange
            var payment = new Payment { Id = 100, SubscriptionId = 10 };
            _paymentRepoMock.Setup(r => r.GetPaymentByIdAsync(100)).ReturnsAsync(payment);

            // Act
            var result = await _paymentService.ProcessPaymentFailureAsync(100, "TRANS123");

            // Assert
            Assert.True(result);
            _paymentRepoMock.Verify(
                r => r.UpdatePaymentStatusAsync(100, "FAILED", "TRANS123"),
                Times.Once
            );
            _paymentRepoMock.Verify(
                r => r.UpdateSubscriptionStatusAsync(10, "CANCELLED"),
                Times.Once
            );
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetActiveSubscriptionPlansAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetActiveSubscriptionPlansAsync_ReturnsList()
        {
            // Arrange
            var plans = new List<SubscriptionPlan> { new SubscriptionPlan { Id = 1 } };
            _paymentRepoMock.Setup(r => r.GetActiveSubscriptionPlansAsync()).ReturnsAsync(plans);

            // Act
            var result = await _paymentService.GetActiveSubscriptionPlansAsync();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        [Trait("CodeModule", "Payment")]
        [Trait("Method", "GetSubscriptionPlanByIdAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task GetSubscriptionPlanByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _paymentRepoMock
                .Setup(r => r.GetSubscriptionPlanByIdAsync(99))
                .ReturnsAsync((SubscriptionPlan)null);

            // Act
            var result = await _paymentService.GetSubscriptionPlanByIdAsync(99);

            // Assert
            Assert.Null(result);
        }
    }
}
