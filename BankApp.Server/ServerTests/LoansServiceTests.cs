using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using NSubstitute;
using NUnit.Framework;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class LoansServiceTests
    {
#pragma warning disable SX1309 // Field names should begin with underscore
        private ILoanRepository mockLoanRepository;
        private LoanService loanService;
#pragma warning restore SX1309 // Field names should begin with underscore

        [SetUp]
        public void SetUp()
        {
            mockLoanRepository = Substitute.For<ILoanRepository>();
            loanService = new LoanService(mockLoanRepository);
        }

        [Test]
        public async Task GetLoanByIdAsync_IdIsZero_ReturnsEmptyLoan()
        {
            Loan result = await loanService.GetLoanByIdAsync(0);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(0));
                Assert.That(result.Principal, Is.EqualTo(0m));
            }
        }

        [Test]
        public async Task GetLoanByIdAsync_ValidId_ReturnsLoanFromRepository()
        {
            Loan loan = CreateLoan();
            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            Loan result = await loanService.GetLoanByIdAsync(loan.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(loan.Id));
        }

        [Test]
        public async Task GetLoansByUserAsync_IdIsZero_ReturnsEmptyList()
        {
            List<Loan> result = await loanService.GetLoansByUserAsync(0);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ApplyForLoanAsync_ValidRequest_CreatesPendingApplication()
        {
            LoanApplicationRequest request = CreateApplicationRequest();
            mockLoanRepository.CreateLoanApplicationAsync(request).Returns(Task.FromResult(99));

            LoanApplication application = await loanService.ApplyForLoanAsync(request);

            Assert.That(application, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(application.UserId, Is.EqualTo(99));
                Assert.That(application.ApplicationStatus, Is.EqualTo(LoanApplicationStatus.Pending));
                Assert.That(application.DesiredAmount, Is.EqualTo(request.DesiredAmount));
            }
        }

        [Test]
        public async Task ProcessApplicationStatusAsync_ActiveLoansExceedLimit_ReturnsRejected()
        {
            LoanApplication application = CreateApplication();

            var activeLoans = new List<Loan>();
            for (int i = 0; i < 5; i++)
            {
                activeLoans.Add(new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m });
            }

            mockLoanRepository.GetLoansByUserAsync(application.UserId).Returns(Task.FromResult(activeLoans));

            var (status, reason) = await loanService.ProcessApplicationStatusAsync(application);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(status, Is.EqualTo(LoanApplicationStatus.Rejected));
                Assert.That(reason, Is.EqualTo("Maximum number of active loans reached."));
            }
        }

        [Test]
        public async Task ProcessApplicationStatusAsync_TotalDebtExceedsLimit_ReturnsRejected()
        {
            LoanApplication application = CreateApplication();
            application.DesiredAmount = 50000m;

            var activeLoans = new List<Loan>
            {
                new Loan { LoanStatus = LoanStatus.Active, OutstandingBalance = 160000m }
            };

            mockLoanRepository.GetLoansByUserAsync(application.UserId).Returns(Task.FromResult(activeLoans));

            var (status, reason) = await loanService.ProcessApplicationStatusAsync(application);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(status, Is.EqualTo(LoanApplicationStatus.Rejected));
                Assert.That(reason, Is.EqualTo("Total debt limit exceeded."));
            }
        }

        [Test]
        public async Task ProcessApplicationStatusAsync_ValidApplication_ReturnsApproved()
        {
            LoanApplication application = CreateApplication();
            mockLoanRepository.GetLoansByUserAsync(application.UserId).Returns(Task.FromResult(new List<Loan>()));

            var (status, reason) = await loanService.ProcessApplicationStatusAsync(application);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(status, Is.EqualTo(LoanApplicationStatus.Approved));
                Assert.That(reason, Is.Null);
            }
            await mockLoanRepository.Received(1).UpdateLoanApplicationStatusAsync(application.UserId, LoanApplicationStatus.Approved, null);
        }

        [Test]
        public void PayInstallmentAsync_LoanNotFound_ThrowsInvalidOperationException()
        {
            mockLoanRepository.GetLoanByIdAsync(1).Returns(Task.FromResult<Loan>(null!));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await loanService.PayInstallmentAsync(1, 100m), "Loan not found.");
        }

        [Test]
        public void PayInstallmentAsync_LoanAlreadyClosed_ThrowsInvalidOperationException()
        {
            Loan loan = CreateLoan();
            loan.LoanStatus = LoanStatus.Passed;
            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await loanService.PayInstallmentAsync(loan.Id, 100m), "This loan is already closed.");
        }

        [Test]
        public void PayInstallmentAsync_PaymentAmountZero_ThrowsArgumentException()
        {
            Loan loan = CreateLoan();
            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            Assert.ThrowsAsync<ArgumentException>(async () => await loanService.PayInstallmentAsync(loan.Id, 0m), "Payment amount must be greater than zero.");
        }

        [Test]
        public void PayInstallmentAsync_CustomAmountBelowMinimum_ThrowsInvalidOperationException()
        {
            Loan loan = CreateLoan();
            loan.MonthlyInstallment = 500m;
            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await loanService.PayInstallmentAsync(loan.Id, 400m), "Payment amount must be at least the minimum installment.");
        }

        [Test]
        public void PayInstallmentAsync_PaymentExceedsBalance_ThrowsInvalidOperationException()
        {
            Loan loan = CreateLoan();
            loan.OutstandingBalance = 1000m;
            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await loanService.PayInstallmentAsync(loan.Id, 1500m), "Payment amount exceeds the outstanding balance.");
        }

        [Test]
        public async Task PayInstallmentAsync_ValidPayment_UpdatesLoan()
        {
            Loan loan = CreateLoan();
            loan.OutstandingBalance = 10000m;
            loan.MonthlyInstallment = 500m;
            loan.RemainingMonths = 20;

            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            await loanService.PayInstallmentAsync(loan.Id, 500m);

            await mockLoanRepository.Received(1).UpdateLoanAfterPaymentAsync(
                loan.UserId,
                Arg.Any<decimal>(),
                Arg.Any<int>(),
                LoanStatus.Active);
        }

        [Test]
        public async Task PayInstallmentAsync_FullPayment_ClosesLoan()
        {
            Loan loan = CreateLoan();
            loan.OutstandingBalance = 500m;
            loan.MonthlyInstallment = 500m;
            loan.RemainingMonths = 1;

            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            await loanService.PayInstallmentAsync(loan.Id, 500m);

            await mockLoanRepository.Received(1).UpdateLoanAfterPaymentAsync(
                loan.UserId,
                0m,
                0,
                LoanStatus.Passed);
        }

        [Test]
        public async Task GetAmortizationAsync_NoRows_GeneratesAndReturnsRows()
        {
            Loan loan = CreateLoan();

            mockLoanRepository.GetAmortizationAsync(loan.Id).Returns(
                Task.FromResult(new List<AmortizationRow>()),
                Task.FromResult(new List<AmortizationRow>
                {
                    new AmortizationRow { DueDate = DateTime.Today.AddDays(1) }
                }));

            mockLoanRepository.GetLoanByIdAsync(loan.Id).Returns(Task.FromResult(loan));

            var rows = await loanService.GetAmortizationAsync(loan.Id);

            Assert.That(rows, Is.Not.Null);
            Assert.That(rows.Count, Is.GreaterThan(0));
            await mockLoanRepository.Received(1).SaveAmortizationAsync(Arg.Any<List<AmortizationRow>>());
        }

        public static Loan CreateLoan()
        {
            return new Loan
            {
                Id = 1,
                UserId = 3,
                LoanType = LoanType.Personal,
                Principal = 10000m,
                OutstandingBalance = 10000m,
                InterestRate = 8.5m,
                MonthlyInstallment = 500m,
                RemainingMonths = 24,
                TermInMonths = 24,
                LoanStatus = LoanStatus.Active,
                StartDate = DateTime.Now
            };
        }

        public static LoanApplicationRequest CreateApplicationRequest()
        {
            return new LoanApplicationRequest
            {
                UserId = 3,
                LoanType = LoanType.Personal,
                DesiredAmount = 10000m,
                PreferredTermMonths = 24,
                Purpose = "Home Renovation"
            };
        }

        public static LoanApplication CreateApplication()
        {
            return new LoanApplication
            {
                UserId = 3,
                LoanType = LoanType.Personal,
                DesiredAmount = 10000m,
                PreferredTermMonths = 24,
                Purpose = "Home Renovation",
                ApplicationStatus = LoanApplicationStatus.Pending
            };
        }
    }
}