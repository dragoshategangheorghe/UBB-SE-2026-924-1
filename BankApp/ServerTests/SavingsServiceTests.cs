using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Entities;
using BankApp.Models.Features.Savings;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using NSubstitute;
using NUnit.Framework;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class SavingsServiceTests
    {
#pragma warning disable SX1309 // Field names should begin with underscore
        private ISavingsRepository mockSavingsRepository;
        private SavingsService savingsService;
#pragma warning restore SX1309 // Field names should begin with underscore

        [SetUp]
        public void SetUp()
        {
            mockSavingsRepository = Substitute.For<ISavingsRepository>();
            savingsService = new SavingsService(mockSavingsRepository);
        }

        [Test]
        public void CreateAccountAsync_ActiveAccountsLimitReached_ThrowsInvalidOperationException()
        {
            var request = CreateCreateSavingsAccountDto();
            var activeAccounts = new List<SavingsAccount>
            {
                new SavingsAccount(), new SavingsAccount(), new SavingsAccount(),
                new SavingsAccount(), new SavingsAccount() // 5 active accounts
            };

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(request.UserIdentificationNumber, false)
                .Returns(Task.FromResult(activeAccounts));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.CreateAccountAsync(request),
                "You cannot have more than 5 active savings accounts.");
        }

        [Test]
        public void CreateAccountAsync_GoalSavingsMissingTargetDate_ThrowsArgumentException()
        {
            var request = CreateCreateSavingsAccountDto();
            request.SavingsType = "GoalSavings";
            request.TargetDate = null;

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(request.UserIdentificationNumber, false)
                .Returns(Task.FromResult(new List<SavingsAccount>()));

            Assert.ThrowsAsync<ArgumentException>(
                async () => await savingsService.CreateAccountAsync(request),
                "GoalSavings accounts require a target date.");
        }

        [Test]
        public void CreateAccountAsync_GoalSavingsTargetDateInPast_ThrowsArgumentException()
        {
            var request = CreateCreateSavingsAccountDto();
            request.SavingsType = "GoalSavings";
            request.TargetDate = DateTime.Today.AddDays(-1);

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(request.UserIdentificationNumber, false)
                .Returns(Task.FromResult(new List<SavingsAccount>()));

            Assert.ThrowsAsync<ArgumentException>(
                async () => await savingsService.CreateAccountAsync(request),
                "Target date must be in the future.");
        }

        [Test]
        public async Task CreateAccountAsync_ValidAccount_ReturnsCreatedAccountWithCorrectApy()
        {
            var request = CreateCreateSavingsAccountDto();
            request.SavingsType = "FixedDeposit"; // APY should be 0.04m

            var expectedAccount = new SavingsAccount { IdentificationNumber = 1, SavingsType = "FixedDeposit" };

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(request.UserIdentificationNumber, false)
                .Returns(Task.FromResult(new List<SavingsAccount>()));

            mockSavingsRepository.CreateSavingsAccountAsync(request, 0.04m)
                .Returns(Task.FromResult(expectedAccount));

            var result = await savingsService.CreateAccountAsync(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IdentificationNumber, Is.EqualTo(1));
        }

        [Test]
        public void GetAccountsAsync_InvalidUserId_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(
                async () => await savingsService.GetAccountsAsync(-1),
                "User ID must be a positive integer.");
        }

        [Test]
        public void DepositAsync_AmountZero_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(
                async () => await savingsService.DepositAsync(1, 0m, "Cash", 1),
                "Deposit amount must be positive.");
        }

        [Test]
        public void DepositAsync_AccountNotFoundOrNotOwned_ThrowsInvalidOperationException()
        {
            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(1, true)
                .Returns(Task.FromResult(new List<SavingsAccount>()));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.DepositAsync(99, 100m, "Cash", 1),
                "Account not found or does not belong to you.");
        }

        [Test]
        public void DepositAsync_AccountClosed_ThrowsInvalidOperationException()
        {
            var account = CreateSavingsAccount();
            account.AccountStatus = "Closed";

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(account.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { account }));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.DepositAsync(account.IdentificationNumber, 100m, "Cash", account.User.Id),
                "Cannot deposit into a closed account.");
        }

        [Test]
        public void DepositAsync_AccountMatured_ThrowsInvalidOperationException()
        {
            var account = CreateSavingsAccount();
            account.SavingsType = "FixedDeposit";
            account.MaturityDate = DateTime.UtcNow.AddDays(-1); // Forces DisplayStatus to evaluate to "Matured"

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(account.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { account }));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.DepositAsync(account.IdentificationNumber, 100m, "Cash", account.User.Id),
                "Cannot deposit into a matured account.");
        }

        [Test]
        public async Task DepositAsync_ValidDeposit_ReturnsResponse()
        {
            var account = CreateSavingsAccount();
            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(account.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { account }));

            var expectedResponse = new DepositResponseDto
            {
                NewBalance = 1600m,
                TransactionId = 12345,
                Timestamp = DateTime.UtcNow
            };

            mockSavingsRepository.DepositAsync(account.IdentificationNumber, 100m, "Cash")
                .Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.DepositAsync(account.IdentificationNumber, 100m, "Cash", account.User.Id);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.NewBalance, Is.EqualTo(1600m));
                Assert.That(result.TransactionId, Is.EqualTo(12345));
            }
        }

        [Test]
        public void CloseAccountAsync_AccountNotFound_ThrowsInvalidOperationException()
        {
            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(1, true)
                .Returns(Task.FromResult(new List<SavingsAccount>()));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.CloseAccountAsync(1, 2, 1),
                "Account not found.");
        }

        [Test]
        public void CloseAccountAsync_DestinationAccountClosed_ThrowsInvalidOperationException()
        {
            var closingAccount = CreateSavingsAccount();
            var destinationAccount = CreateSavingsAccount();
            destinationAccount.IdentificationNumber = 2;
            destinationAccount.AccountStatus = "Closed";

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(closingAccount.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { closingAccount, destinationAccount }));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.CloseAccountAsync(closingAccount.IdentificationNumber, destinationAccount.IdentificationNumber, closingAccount.User.Id),
                "Cannot transfer to a closed account.");
        }

        [Test]
        public async Task CloseAccountAsync_EarlyClosureFixedDeposit_AppliesPenalty()
        {
            var closingAccount = CreateSavingsAccount();
            closingAccount.SavingsType = "FixedDeposit";
            closingAccount.MaturityDate = DateTime.UtcNow.AddDays(30); // In the future -> triggers penalty
            closingAccount.Balance = 1000m;

            var destinationAccount = CreateSavingsAccount();
            destinationAccount.IdentificationNumber = 2;

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(closingAccount.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { closingAccount, destinationAccount }));

            var expectedResponse = new ClosureResultDto { Success = true };

            // Penalty = 1000 * 0.02 = 20. Transfer Amount = 1000 - 20 = 980
            mockSavingsRepository.CloseSavingsAccountAsync(closingAccount.IdentificationNumber, destinationAccount.IdentificationNumber, 980m, 20m)
                .Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(closingAccount.IdentificationNumber, destinationAccount.IdentificationNumber, closingAccount.User.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void WithdrawAsync_AmountZero_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(
                async () => await savingsService.WithdrawAsync(1, 0m, "ATM", 1),
                "Withdrawal amount must be positive.");
        }

        [Test]
        public void WithdrawAsync_AmountExceedsBalance_ThrowsInvalidOperationException()
        {
            var account = CreateSavingsAccount();
            account.Balance = 100m;

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(account.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { account }));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.WithdrawAsync(account.IdentificationNumber, 150m, "ATM", account.User.Id),
                "Insufficient balance.");
        }

        [Test]
        public void WithdrawAsync_EarlyWithdrawalPenaltyExceedsBalance_ThrowsInvalidOperationException()
        {
            var account = CreateSavingsAccount();
            account.SavingsType = "FixedDeposit";
            account.MaturityDate = DateTime.UtcNow.AddDays(30);
            account.Balance = 100m;

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(account.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { account }));

            // Attempt to withdraw 99m. Penalty is 99 * 0.02 = 1.98m. Total needed: 100.98m. Available: 100m.
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await savingsService.WithdrawAsync(account.IdentificationNumber, 99m, "Transfer", account.User.Id),
                "Insufficient balance after penalty.");
        }

        [Test]
        public async Task WithdrawAsync_ValidWithdrawal_ReturnsResponse()
        {
            var account = CreateSavingsAccount();
            account.Balance = 500m;

            mockSavingsRepository.GetSavingsAccountsByUserIdAsync(account.User.Id, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { account }));

            var expectedResponse = new WithdrawResponseDto { Success = true };
            mockSavingsRepository.WithdrawAsync(account.IdentificationNumber, 100m, "ATM", 0m)
                .Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.WithdrawAsync(account.IdentificationNumber, 100m, "ATM", account.User.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void GetTransactionsAsync_PageLessThanOne_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(
                async () => await savingsService.GetTransactionsAsync(1, "All", 0, 20),
                "Page must be greater than or equal to one.");
        }

        [Test]
        public async Task GetTransactionsAsync_InvalidPageSize_DefaultsTo20()
        {
            mockSavingsRepository.GetTransactionsPagedAsync(1, "All", 1, 20)
                .Returns(Task.FromResult((new List<SavingsTransaction>(), 0)));

            // Act with pageSize = 150 (above MAX_PAGE_SIZE = 100)
            await savingsService.GetTransactionsAsync(1, "All", 1, 150);

            // Assert that the repository was called with the default page size of 20
            await mockSavingsRepository.Received(1).GetTransactionsPagedAsync(1, "All", 1, 20);
        }

        public static SavingsAccount CreateSavingsAccount()
        {
            return new SavingsAccount
            {
                IdentificationNumber = 1,
                User = new User { Id = 3 }, // Assigned using the navigation property instead of a standalone ID
                AccountName = "Emergency Fund",
                AccountStatus = "Open",
                SavingsType = "Default",
                Balance = 1500m,
                AnnualPercentageYield = 0.02m
            };
        }

        public static CreateSavingsAccountDto CreateCreateSavingsAccountDto()
        {
            return new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 3, // DTO inherently has this property
                AccountName = "Vacation Fund",
                SavingsType = "Default",
                InitialDeposit = 500m,
                TargetAmount = 2000m,
                TargetDate = DateTime.UtcNow.AddMonths(6)
            };
        }
    }
}