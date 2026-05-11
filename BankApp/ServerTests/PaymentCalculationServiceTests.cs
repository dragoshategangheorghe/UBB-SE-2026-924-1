using System;
using BankApp.Server.Services.Implementations;
using NUnit.Framework;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class PaymentCalculationServiceTests
    {
        private PaymentCalculationService _paymentCalculationService;

        [SetUp]
        public void SetUp()
        {
            _paymentCalculationService = new PaymentCalculationService();
        }

        [Test]
        public void CalculatePaymentPreview_StandardPayment_ReducesBalanceAndMonthsCorrectly()
        {
            var (balance, months) = _paymentCalculationService.CalculatePaymentPreview(
                monthlyInstallment: 500m,
                outstandingBalance: 2000m,
                remainingMonths: 4,
                isStandardPayment: true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(balance, Is.EqualTo(1500m));
                Assert.That(months, Is.EqualTo(3));
            }
        }

        [Test]
        public void CalculatePaymentPreview_CustomPaymentLargerThanInstallment_CalculatesCorrectly()
        {
            var (balance, months) = _paymentCalculationService.CalculatePaymentPreview(
                monthlyInstallment: 500m,
                outstandingBalance: 2000m,
                remainingMonths: 4,
                isStandardPayment: false,
                customPaymentAmount: 1200m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(balance, Is.EqualTo(800m));
                Assert.That(months, Is.EqualTo(2));
            }
        }

        [Test]
        public void CalculatePaymentPreview_FullPayoff_SetsBalanceAndMonthsToZero()
        {
            var (balance, months) = _paymentCalculationService.CalculatePaymentPreview(
                monthlyInstallment: 500m,
                outstandingBalance: 2000m,
                remainingMonths: 4,
                isStandardPayment: false,
                customPaymentAmount: 2500m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(balance, Is.EqualTo(0m));
                Assert.That(months, Is.EqualTo(0));
            }
        }

        [Test]
        public void ParsePaymentAmount_ValidString_ReturnsParsedDecimal()
        {
            var (success, amount) = _paymentCalculationService.ParsePaymentAmount("150.50");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(success, Is.True);
                Assert.That(amount, Is.EqualTo(150.50m));
            }
        }

        [Test]
        public void ParsePaymentAmount_InvalidString_ReturnsFalse()
        {
            var (success, amount) = _paymentCalculationService.ParsePaymentAmount("invalid_number");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(success, Is.False);
                Assert.That(amount, Is.EqualTo(0m));
            }
        }

        [Test]
        public void ValidatePaymentAmount_AmountLessThanZero_ReturnsInvalid()
        {
            var (isValid, message) = _paymentCalculationService.ValidatePaymentAmount(0m, 1000m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(message, Is.EqualTo("Payment amount must be greater than zero."));
            }
        }

        [Test]
        public void ValidatePaymentAmount_AmountExceedsBalance_ReturnsInvalid()
        {
            var (isValid, message) = _paymentCalculationService.ValidatePaymentAmount(1500m, 1000m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(message, Does.Contain("Payment amount cannot exceed outstanding balance"));
            }
        }

        [Test]
        public void GetInitialCustomAmount_CustomAmountExceedsBalance_CapsAtOutstandingBalance()
        {
            decimal result = _paymentCalculationService.GetInitialCustomAmount(
                monthlyInstallment: 500m,
                outstandingBalance: 1000m,
                currentCustomAmount: 1500d);

            Assert.That(result, Is.EqualTo(1000m));
        }
    }
}