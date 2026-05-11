using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using BankApp.Client.Services.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.Tests.Services
{
    public class SavingsServiceTests
    {
        private readonly Mock<ISavingsRepoProxy> mockSavingsRepoProxy;
        private readonly Mock<ISavingsUiRulesRepoProxy> mockSavingsUiRulesProxy;
        private readonly Mock<ISavingsPresentationRepoProxy> mockSavingsPresentationProxy;
        private readonly Mock<ISavingsWorkflowRepoProxy> mockSavingsWorkflowProxy;
        private readonly SavingsService savingsService;

        public SavingsServiceTests()
        {
            mockSavingsRepoProxy = new Mock<ISavingsRepoProxy>();
            mockSavingsUiRulesProxy = new Mock<ISavingsUiRulesRepoProxy>();
            mockSavingsPresentationProxy = new Mock<ISavingsPresentationRepoProxy>();
            mockSavingsWorkflowProxy = new Mock<ISavingsWorkflowRepoProxy>();

            savingsService = new SavingsService(
                mockSavingsRepoProxy.Object,
                mockSavingsUiRulesProxy.Object,
                mockSavingsPresentationProxy.Object,
                mockSavingsWorkflowProxy.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ActiveAccountsLimitReached_ThrowsInvalidOperationException()
        {
            CreateSavingsAccountDto newAccountRequestData = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1
            };
            List<SavingsAccount> maximumAllowedAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount(), new SavingsAccount(), new SavingsAccount(),
                new SavingsAccount(), new SavingsAccount()
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(newAccountRequestData.UserIdentificationNumber, false))
                .ReturnsAsync(maximumAllowedAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.CreateAccountAsync(newAccountRequestData));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsHasMissingTargetDate_ThrowsArgumentException()
        {
            CreateSavingsAccountDto invalidGoalSavingsRequestData = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "GoalSavings",
                TargetDate = null
            };
            List<SavingsAccount> currentEmptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(invalidGoalSavingsRequestData.UserIdentificationNumber, false))
                .ReturnsAsync(currentEmptyAccountsList);

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.CreateAccountAsync(invalidGoalSavingsRequestData));
        }

        [Fact]
        public async Task DepositAsync_NegativeDepositAmountProvided_ThrowsArgumentException()
        {
            int targetAccountIdentificationNumber = 1;
            decimal negativeDepositAmount = -100m;
            int requestingUserIdentificationNumber = 1;
            string fundingSourceLabel = "External Transfer";

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.DepositAsync(targetAccountIdentificationNumber, negativeDepositAmount, fundingSourceLabel, requestingUserIdentificationNumber));
        }

        [Fact]
        public async Task DepositAsync_TargetAccountIsClosed_ThrowsInvalidOperationException()
        {
            int targetAccountIdentificationNumber = 1;
            decimal validDepositAmount = 100m;
            int requestingUserIdentificationNumber = 1;
            string fundingSourceLabel = "External Transfer";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountIdentificationNumber, AccountStatus = "Closed" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserIdentificationNumber, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.DepositAsync(targetAccountIdentificationNumber, validDepositAmount, fundingSourceLabel, requestingUserIdentificationNumber));
        }

        [Fact]
        public async Task WithdrawAsync_RequestedAmountExceedsAccountBalance_ThrowsInvalidOperationException()
        {
            int targetAccountIdentificationNumber = 1;
            decimal excessiveWithdrawalAmount = 5000m;
            string withdrawalDestinationLabel = "Checking";
            int requestingUserIdentificationNumber = 1;

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountIdentificationNumber, AccountStatus = "Active", Balance = 1000m }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserIdentificationNumber, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.WithdrawAsync(targetAccountIdentificationNumber, excessiveWithdrawalAmount, withdrawalDestinationLabel, requestingUserIdentificationNumber));
        }

        [Fact]
        public async Task CloseAccountAsync_DestinationAccountIsClosed_ThrowsInvalidOperationException()
        {
            int accountToCloseIdentificationNumber = 1;
            int destinationAccountIdentificationNumber = 2;
            int requestingUserIdentificationNumber = 1;

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = accountToCloseIdentificationNumber, AccountStatus = "Active" },
                new SavingsAccount { IdentificationNumber = destinationAccountIdentificationNumber, AccountStatus = "Closed" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserIdentificationNumber, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.CloseAccountAsync(accountToCloseIdentificationNumber, destinationAccountIdentificationNumber, requestingUserIdentificationNumber));
        }

        [Fact]
        public async Task GetPenaltyDecimalFor_EarlyClosureCase_ReturnsExpectedClosurePenaltyRate()
        {
            string earlyClosurePenaltyCase = "EarlyClosure";
            decimal expectedPenaltyRatePercentage = 0.02m;

            decimal calculatedPenaltyRate = await savingsService.GetPenaltyDecimalFor(earlyClosurePenaltyCase);

            Assert.Equal(expectedPenaltyRatePercentage, calculatedPenaltyRate);
        }
    }
}