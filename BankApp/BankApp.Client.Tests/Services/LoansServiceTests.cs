using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

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

        [Fact]
        public async Task PayInstallmentAsync_LoanDoesNotExist_ThrowsInvalidOperationException()
        {
            int targetLoanId = 1;

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanId))
                .ReturnsAsync((Loan)null!);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                loansService.PayInstallmentAsync(targetLoanId, null));

        }


        [Fact]
        public async Task PayInstallmentAsync_ActiveLoanHasNoRemainingMonths_ThrowsInvalidOperationException()
        {
            int targetLoanId = 1;
            Loan activeLoanWithoutRemainingMonths = new Loan
            {
                UserId = 1,
                RemainingMonths = 0,
                LoanStatus = LoanStatus.Active,
                MonthlyInstallment = 100m,
                OutstandingBalance = 500m
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanId))
                .ReturnsAsync(activeLoanWithoutRemainingMonths);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                loansService.PayInstallmentAsync(targetLoanId, null));

        }
        [Fact]
        public async Task PayInstallmentAsync_CustomPaymentBelowMonthlyInstallment_ThrowsInvalidOperationException()
        {
            int targetLoanId = 1;
            decimal customPaymentAmount = 50m;
            Loan activeLoanRecord = new Loan
            {
                UserId = 1,
                RemainingMonths = 12,
                LoanStatus = LoanStatus.Active,
                MonthlyInstallment = 100m,
                OutstandingBalance = 1000m
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanId))
                .ReturnsAsync(activeLoanRecord);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                loansService.PayInstallmentAsync(targetLoanId, customPaymentAmount));
        }

        [Fact]
        public async Task PayInstallmentAsync_StandardPayment_UpdatesLoanWithOneFewerMonth()
        {
            int targetLoanId = 1;
            Loan activeLoanRecord = new Loan
            {
                UserId = 1,
                RemainingMonths = 12,
                LoanStatus = LoanStatus.Active,
                MonthlyInstallment = 100m,
                OutstandingBalance = 1000m
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanId))
                .ReturnsAsync(activeLoanRecord);

            await loansService.PayInstallmentAsync(targetLoanId, null);

            mockLoansRepoProxy.Verify(proxy => proxy.UpdateLoanAfterPaymentAsync(
                targetLoanId,
                900m,
                11,
                LoanStatus.Active), Times.Once);
        }
        [Fact]
        public async Task PayInstallmentAsync_CustomPaymentAtLeastMonthlyInstallment_UpdatesLoanByPaidMonths()
        {
            int targetLoanId = 1;
            decimal customPaymentAmount = 250m;
            Loan activeLoanRecord = new Loan
            {
                UserId = 1,
                RemainingMonths = 12,
                LoanStatus = LoanStatus.Active,
                MonthlyInstallment = 100m,
                OutstandingBalance = 1000m
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(targetLoanId))
                .ReturnsAsync(activeLoanRecord);

            await loansService.PayInstallmentAsync(targetLoanId, customPaymentAmount);

            mockLoansRepoProxy.Verify(proxy => proxy.UpdateLoanAfterPaymentAsync(
                targetLoanId,
                750m,
                10,
                LoanStatus.Active), Times.Once);
        }
        [Fact]
        public async Task GetLoansByUserAsync_ValidUserId_ReturnsLoansFromRepoProxy()
        {
            int userId = 1;
            List<Loan> expectedLoans = new List<Loan>
            {
                new Loan
                {
                    UserId = userId,
                    LoanStatus = LoanStatus.Active,
                    OutstandingBalance = 1000m
                },
                new Loan
                {
                    UserId = userId,
                    LoanStatus = LoanStatus.Passed,
                    OutstandingBalance = 0m
                }
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoansByUserAsync(userId))
                .ReturnsAsync(expectedLoans);

            List<Loan> actualLoans = await loansService.GetLoansByUserAsync(userId);

            Assert.Equal(expectedLoans, actualLoans);
        }

        [Fact]
        public void GetLoanEstimate_ValidPersonalLoanRequest_ReturnsCalculatedEstimate()
        {
            LoanApplicationRequest request = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 12000m,
                PreferredTermMonths = 12,
                Purpose = "Test"
            };

            LoanEstimate estimate = loansService.GetLoanEstimate(request);

            Assert.Equal(8.5m, estimate.IndicativeRate);
            Assert.True(estimate.MonthlyInstallment > 0m);
            Assert.Equal(estimate.MonthlyInstallment * request.PreferredTermMonths, estimate.TotalRepayable);
        }
        [Fact]
        public async Task GetAmortizationAsync_ExistingRows_ReturnsRowsAndMarksFirstCurrentOrFutureRow()
        {
            int loanId = 1;
            List<AmortizationRow> amortizationRows = new List<AmortizationRow>
            {
                new AmortizationRow
                {
                    LoanId = loanId,
                    DueDate = DateTime.Today.AddDays(-1)
                },
                new AmortizationRow
                {
                    LoanId = loanId,
                    DueDate = DateTime.Today
                },
                new AmortizationRow
                {
                    LoanId = loanId,
                    DueDate = DateTime.Today.AddDays(1)
                }
            };

            mockLoansRepoProxy.Setup(proxy => proxy.GetAmortizationAsync(loanId))
                .ReturnsAsync(amortizationRows);

            List<AmortizationRow> result = await loansService.GetAmortizationAsync(loanId);

            Assert.Same(amortizationRows, result);
            Assert.False(result[0].IsCurrent);
            Assert.True(result[1].IsCurrent);
            Assert.False(result[2].IsCurrent);

            mockLoansRepoProxy.Verify(proxy => proxy.GetLoanByIdAsync(It.IsAny<int>()), Times.Never);
            mockLoansRepoProxy.Verify(proxy => proxy.SaveAmortizationAsync(
                It.IsAny<int>(),
                It.IsAny<List<AmortizationRow>>()), Times.Never);
        }

        [Fact]
        public async Task GetAmortizationAsync_NullRows_GeneratesSavesAndReturnsRows()
        {
            int loanId = 1;
            Loan loan = new Loan
            {
                Id = loanId,
                Principal = 12000m,
                OutstandingBalance = 12000m,
                InterestRate = 8.5m,
                TermInMonths = 12,
                StartDate = DateTime.Today,
                MonthlyInstallment = 1000m
            };

            List<AmortizationRow> generatedRows = new List<AmortizationRow>
            {
                new AmortizationRow
                {
                    LoanId = loanId,
                    DueDate = DateTime.Today.AddMonths(1)
                }
            };

            mockLoansRepoProxy.SetupSequence(proxy => proxy.GetAmortizationAsync(loanId))
                .ReturnsAsync((List<AmortizationRow>)null!)
                .ReturnsAsync(generatedRows);

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(loanId))
                .ReturnsAsync(loan);

            List<AmortizationRow> result = await loansService.GetAmortizationAsync(loanId);

            Assert.Same(generatedRows, result);
            Assert.True(result[0].IsCurrent);

            mockLoansRepoProxy.Verify(proxy => proxy.GetLoanByIdAsync(loanId), Times.Once);
            mockLoansRepoProxy.Verify(proxy => proxy.SaveAmortizationAsync(
                loanId,
                It.Is<List<AmortizationRow>>(rows => rows.Count == loan.TermInMonths)), Times.Once);
        }
        [Fact]
        public async Task SubmitLoanApplicationAsync_UserHasMaximumActiveLoans_ReturnsRejectedResult()
        {
            int applicationId = 10;
            LoanApplicationRequest request = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 1000m,
                PreferredTermMonths = 12,
                Purpose = "test"
            };

            List<Loan> existingLoans = new List<Loan>
            {
                new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m }
            };

            mockLoansRepoProxy.Setup(proxy => proxy.CreateLoanApplicationAsync(request))
                .ReturnsAsync(applicationId);

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoansByUserAsync(request.UserId))
                .ReturnsAsync(existingLoans);

            LoanApplicationResult result = await loansService.SubmitLoanApplicationAsync(request);

            Assert.Equal(LoanApplicationStatus.Rejected, result.Status);
            Assert.Equal("Maximum number of active loans reached.", result.RejectionReason);

        }
        [Fact]
        public async Task SubmitLoanApplicationAsync_TotalDebtLimitReached_ReturnsRejectedResult()
        {
            int applicationId = 10;
            LoanApplicationRequest request = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 1000m,
                PreferredTermMonths = 12,
                Purpose = "test"
            };

            List<Loan> existingLoans = new List<Loan>
            {
                new Loan
                {
                    LoanStatus = LoanStatus.Active,
                    OutstandingBalance = 199000m
                }
            };

            mockLoansRepoProxy.Setup(proxy => proxy.CreateLoanApplicationAsync(request))
                .ReturnsAsync(applicationId);

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoansByUserAsync(request.UserId))
                .ReturnsAsync(existingLoans);

            LoanApplicationResult result = await loansService.SubmitLoanApplicationAsync(request);

            Assert.Equal(LoanApplicationStatus.Rejected, result.Status);
            Assert.Equal("Total debt limit exceeded.", result.RejectionReason);
        }
        [Fact]
        public async Task SubmitLoanApplicationAsync_ApplicationPassesEvaluation_ReturnsApprovedResultAndCreatesLoan()
        {
            int applicationId = 10;
            int createdLoanId = 20;
            LoanApplicationRequest request = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 12000m,
                PreferredTermMonths = 12,
                Purpose = "Personal expenses"
            };

            Loan createdLoanRecord = new Loan
            {
                Id = createdLoanId,
                UserId = request.UserId,
                Principal = request.DesiredAmount,
                OutstandingBalance = request.DesiredAmount,
                InterestRate = 8.5m,
                TermInMonths = request.PreferredTermMonths,
                StartDate = DateTime.Today
            };

            mockLoansRepoProxy.Setup(proxy => proxy.CreateLoanApplicationAsync(request))
                .ReturnsAsync(applicationId);

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoansByUserAsync(request.UserId))
                .ReturnsAsync(new List<Loan>());

            mockLoansRepoProxy.Setup(proxy => proxy.CreateLoanAsync(It.IsAny<Loan>()))
                .ReturnsAsync(createdLoanId);

            mockLoansRepoProxy.Setup(proxy => proxy.GetLoanByIdAsync(createdLoanId))
                .ReturnsAsync(createdLoanRecord);

            LoanApplicationResult result = await loansService.SubmitLoanApplicationAsync(request);

            Assert.Equal(LoanApplicationStatus.Approved, result.Status);

            mockLoansRepoProxy.Verify(proxy => proxy.CreateLoanAsync(It.Is<Loan>(loan =>
                loan.UserId == request.UserId &&
                loan.LoanType == request.LoanType &&
                loan.Principal == request.DesiredAmount &&
                loan.OutstandingBalance == request.DesiredAmount &&
                loan.InterestRate == 8.5m &&
                loan.MonthlyInstallment > 0m &&
                loan.RemainingMonths == request.PreferredTermMonths &&
                loan.LoanStatus == LoanStatus.Active &&
                loan.TermInMonths == request.PreferredTermMonths)), Times.Once);
        }
        [Theory]
        [InlineData(LoanType.Personal, 8.5)]
        [InlineData(LoanType.Mortgage, 4.5)]
        [InlineData(LoanType.Student, 3.0)]
        [InlineData(LoanType.Auto, 6.5)]
        public void GetLoanEstimate_ValidLoanType_ReturnsExpectedInterestRate(LoanType loanType, double expectedInterestRate)
        {
            LoanApplicationRequest request = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = loanType,
                DesiredAmount = 12000m,
                PreferredTermMonths = 12,
                Purpose = "Loan purpose"
            };

            LoanEstimate estimate = loansService.GetLoanEstimate(request);

            Assert.Equal((decimal)expectedInterestRate, estimate.IndicativeRate);
        }

        [Fact]
        public void ParseCustomPaymentAmount_WhitespaceInput_ReturnsNull()
        {
            decimal? result = loansService.ParseCustomPaymentAmount("   ");

            Assert.Null(result);
        }
        [Fact]
        public void ParseCustomPaymentAmount_InvalidInput_ReturnsNull()
        {
            decimal? result = loansService.ParseCustomPaymentAmount("abc");

            Assert.Null(result);
        }
        [Fact]
        public void NormalizeCustomPaymentAmount_NoCustomAmount_ReturnsMonthlyInstallment()
        {
            Loan loan = new Loan
            {
                MonthlyInstallment = 100m,
                OutstandingBalance = 500m
            };

            decimal result = loansService.NormalizeCustomPaymentAmount(loan, null);

            Assert.Equal(100m, result);
        }
        [Fact]
        public void NormalizeCustomPaymentAmount_CustomAmountWithinBalance_ReturnsCustomAmount()
        {
            Loan loan = new Loan
            {
                MonthlyInstallment = 100m,
                OutstandingBalance = 500m
            };

            decimal result = loansService.NormalizeCustomPaymentAmount(loan, 250m);

            Assert.Equal(250m, result);
        }
        [Fact]
        public void NormalizeCustomPaymentAmount_CustomAmountAboveOutstandingBalance_ReturnsOutstandingBalance()
        {
            Loan loan = new Loan
            {
                MonthlyInstallment = 100m,
                OutstandingBalance = 500m
            };

            decimal result = loansService.NormalizeCustomPaymentAmount(loan, 750m);

            Assert.Equal(500m, result);
        }

    }
}