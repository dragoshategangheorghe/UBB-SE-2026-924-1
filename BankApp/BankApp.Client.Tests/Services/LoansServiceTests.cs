using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using BankApp.Client.Services.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.Features.Loans;
using BankApp.Models.Enums;

namespace BankApp.Client.Tests.Services
{
    public class LoansServiceTests
    {
        private readonly Mock<ILoansRepoProxy> mockLoansRepoProxy;
        private readonly Mock<ILoanDialogStateRepoProxy> mockLoanDialogStateProxy;
        private readonly Mock<ILoanApplicationPresentationRepoProxy> mockLoanPresentationProxy;
        private readonly LoansService loansService;

        public LoansServiceTests()
        {
            mockLoansRepoProxy = new Mock<ILoansRepoProxy>();
            mockLoanDialogStateProxy = new Mock<ILoanDialogStateRepoProxy>();
            mockLoanPresentationProxy = new Mock<ILoanApplicationPresentationRepoProxy>();

            loansService = new LoansService(
                mockLoansRepoProxy.Object,
                mockLoanDialogStateProxy.Object,
                mockLoanPresentationProxy.Object);
        }

        [Fact]
        public async Task PayInstallmentAsync_TargetLoanIsAlreadyPassed_ThrowsInvalidOperationException()
        {
            int targetLoanIdentificationNumber = 1;
            decimal customPaymentAmount = 100m;
            Loan passedLoanRecord = new Loan
            {
                UserId = 1,
                RemainingMonths = 0,
                LoanStatus = LoanStatus.Passed
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanIdentificationNumber))
                .ReturnsAsync(passedLoanRecord);

            await Assert.ThrowsAsync<InvalidOperationException>(() => loansService.PayInstallmentAsync(targetLoanIdentificationNumber, customPaymentAmount));
        }

        [Fact]
        public async Task PayInstallmentAsync_PaymentAmountIsNegative_ThrowsArgumentException()
        {
            int targetLoanIdentificationNumber = 1;
            decimal negativePaymentAmount = -50m;
            Loan activeLoanRecord = new Loan
            {
                UserId = 1,
                RemainingMonths = 12,
                LoanStatus = LoanStatus.Active,
                MonthlyInstallment = 100m
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanIdentificationNumber))
                .ReturnsAsync(activeLoanRecord);

            await Assert.ThrowsAsync<ArgumentException>(() => loansService.PayInstallmentAsync(targetLoanIdentificationNumber, negativePaymentAmount));
        }

        [Fact]
        public async Task PayInstallmentAsync_PaymentAmountExceedsOutstandingBalance_ThrowsInvalidOperationException()
        {
            int targetLoanIdentificationNumber = 1;
            decimal excessivePaymentAmount = 5000m;
            Loan activeLoanRecord = new Loan
            {
                UserId = 1,
                RemainingMonths = 12,
                LoanStatus = LoanStatus.Active,
                MonthlyInstallment = 100m,
                OutstandingBalance = 2000m
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanIdentificationNumber))
                .ReturnsAsync(activeLoanRecord);

            await Assert.ThrowsAsync<InvalidOperationException>(() => loansService.PayInstallmentAsync(targetLoanIdentificationNumber, excessivePaymentAmount));
        }

        [Fact]
        public void ParseCustomPaymentAmount_ValidNumericString_ReturnsParsedDecimalAmount()
        {
            string validPaymentInputText = "250.50";
            decimal expectedParsedPaymentAmount = 250.50m;

            decimal? actualParsedResult = loansService.ParseCustomPaymentAmount(validPaymentInputText);

            Assert.Equal(expectedParsedPaymentAmount, actualParsedResult);
        }

        [Fact]
        public void GetRepaymentProgress_PartiallyPaidLoan_CalculatesAccuratePercentage()
        {
            Loan partiallyRepaidLoan = new Loan
            {
                Principal = 10000m,
                OutstandingBalance = 2500m
            };
            double expectedRepaymentPercentage = 75.0;

            double calculatedRepaymentProgress = loansService.GetRepaymentProgress(partiallyRepaidLoan);

            Assert.Equal(expectedRepaymentPercentage, calculatedRepaymentProgress);
        }
    }
}