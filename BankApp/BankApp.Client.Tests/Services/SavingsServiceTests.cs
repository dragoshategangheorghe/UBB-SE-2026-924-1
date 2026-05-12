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

        // Note: I copied the constants over from SavingsServices
        //  in order not to have to deal with magic numbers here as well
        // TODO: later move all of the references to these numbers into one constants file
        private const decimal FixedDepositApy = 0.04m;
        private const decimal GoalSavingsApy = 0.03m;
        private const decimal HighYieldApy = 0.03m;
        private const decimal DefaultApy = 0.02m;

        private const decimal DecimalEarlyWithdrawalPenalty = 0.02m;
        private const decimal DecimalEarlyClosurePenalty = 0.02m;

        private const int MinPage = 1;
        private const int MaxPageSize = 100;
        private const int MinUserId = 0;
        private const int DefaultPageSize = 20;

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
            CreateSavingsAccountDto invalidCreateSavingsRequestDataWithGoalSavings = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "GoalSavings",
                TargetDate = null
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(invalidCreateSavingsRequestDataWithGoalSavings.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.CreateAccountAsync(invalidCreateSavingsRequestDataWithGoalSavings));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsHasTargetDateInPast_ThrowsArgumentException()
        {
            var invalidCreateSavingsRequestDataWithGoalSavings = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "GoalSavings",
                TargetDate = DateTime.Now - new TimeSpan(1, 0, 0, 0),
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(invalidCreateSavingsRequestDataWithGoalSavings.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.CreateAccountAsync(invalidCreateSavingsRequestDataWithGoalSavings));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsHasNoTargetAmount_ThrowsArgumentException()
        {
            var invalidCreateSavingsRequestDataWithGoalSavings = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "GoalSavings",
                TargetDate = DateTime.Now + new TimeSpan(1, 0, 0, 0),
                TargetAmount = null,
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(invalidCreateSavingsRequestDataWithGoalSavings.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.CreateAccountAsync(invalidCreateSavingsRequestDataWithGoalSavings));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsHasNegativeTargetAmount_ThrowsArgumentException()
        {
            var invalidCreateSavingsRequestDataWithGoalSavings = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "GoalSavings",
                TargetDate = DateTime.Now + new TimeSpan(1, 0, 0, 0),
                TargetAmount = -0.1m,
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(invalidCreateSavingsRequestDataWithGoalSavings.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.CreateAccountAsync(invalidCreateSavingsRequestDataWithGoalSavings));
        }

        [Fact]
        public async Task CreateAccountAsync_RequestsCreatingSavingsAccountWithFixedDeposit_SetsAnnualPercentageYieldToFixedDepositPercentage()
        {
            var createSavingsRequestDataWithFixedDeposit = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "FixedDeposit",
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(createSavingsRequestDataWithFixedDeposit.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await savingsService.CreateAccountAsync(createSavingsRequestDataWithFixedDeposit);

            mockSavingsRepoProxy.Verify(proxy => proxy.CreateSavingsAccountAsync(createSavingsRequestDataWithFixedDeposit, FixedDepositApy), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_RequestsCreatingSavingsAccountWithGoalSavings_SetsAnnualPercentageYieldToGoalSavingsPercentage()
        {
            var createSavingsRequestDataWithGoalSavings = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "GoalSavings",
                TargetDate = DateTime.Now + new TimeSpan(1, 0, 0, 0),
                TargetAmount = 1m,
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(createSavingsRequestDataWithGoalSavings.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await savingsService.CreateAccountAsync(createSavingsRequestDataWithGoalSavings);

            mockSavingsRepoProxy.Verify(proxy => proxy.CreateSavingsAccountAsync(createSavingsRequestDataWithGoalSavings, GoalSavingsApy), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_RequestsCreatingSavingsAccountWithHighYield_SetsAnnualPercentageYieldToHighYieldPercentage()
        {
            var createSavingsAccountRequestDataWithHighYield = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "HighYield",
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(createSavingsAccountRequestDataWithHighYield.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await savingsService.CreateAccountAsync(createSavingsAccountRequestDataWithHighYield);

            mockSavingsRepoProxy.Verify(proxy => proxy.CreateSavingsAccountAsync(createSavingsAccountRequestDataWithHighYield, HighYieldApy), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_RequestsCreatingSavingsAccountWithNoSpecificType_SetsAnnualPercentageYieldToDefaultPercentage()
        {
            var createSavingsAccountRequestDataWithNoSpecificType = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "Default",
            };
            List<SavingsAccount> emptyAccountsList = new List<SavingsAccount>();

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(createSavingsAccountRequestDataWithNoSpecificType.UserIdentificationNumber, false))
                .ReturnsAsync(emptyAccountsList);

            await savingsService.CreateAccountAsync(createSavingsAccountRequestDataWithNoSpecificType);

            mockSavingsRepoProxy.Verify(proxy => proxy.CreateSavingsAccountAsync(createSavingsAccountRequestDataWithNoSpecificType, DefaultApy), Times.Once);
        }

        [Fact]
        public async Task GetAccountsAsync_UserIdIsNegativeInteger_ThrowsArgumentException()
        {
            int invalidUserId = -1;

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.GetAccountsAsync(invalidUserId));
        }

        [Fact]
        public async Task GetAccountsAsync_UserIdIsValid_ReturnsSavingsAccounts()
        {
            int userId = 0;

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(userId, false))
                .ReturnsAsync(new List<SavingsAccount>() { new SavingsAccount { IdentificationNumber = 0 } });

            var resultingSavingsAccounts = await savingsService.GetAccountsAsync(userId);

            Assert.True(resultingSavingsAccounts.Any());
        }

        [Fact]
        public async Task DepositAsync_NegativeDepositAmountProvided_ThrowsArgumentException()
        {
            int targetAccountId = 1;
            decimal negativeDepositAmount = -100m;
            int requestingUserId = 1;
            string fundingSourceLabel = "External Transfer";

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.DepositAsync(targetAccountId, negativeDepositAmount, fundingSourceLabel, requestingUserId));
        }

        [Fact]
        public async Task DepositAsync_NoSavingsAccountWithGivenAccountIdFoundOnUserId_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal depositAmount = 100m;
            int requestingUserId = 2;
            string fundingSourceLabel = "External Transfer";

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, false))
                .ReturnsAsync(new List<SavingsAccount>() { new SavingsAccount { IdentificationNumber = 0 } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.DepositAsync(targetAccountId, depositAmount, fundingSourceLabel, requestingUserId));
        }

        [Fact]
        public async Task DepositAsync_TargetAccountIsClosed_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal depositAmount = 100m;
            int requestingUserId = 1;
            string fundingSourceLabel = "External Transfer";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountId, AccountStatus = "Closed" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.DepositAsync(targetAccountId, depositAmount, fundingSourceLabel, requestingUserId));
        }

        [Fact]
        public async Task DepositAsync_TargetAccountIsMatured_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal depositAmount = 100m;
            int requestingUserId = 1;
            string fundingSourceLabel = "External Transfer";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountId, AccountStatus = "Matured" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.DepositAsync(targetAccountId, depositAmount, fundingSourceLabel, requestingUserId));
        }

        [Fact]
        public async Task DepositAsync_SuccessfulDepositRequest_CallsDepositFunctionInRepo()
        {
            int targetAccountId = 1;
            decimal depositAmount = 100m;
            int requestingUserId = 1;
            string fundingSourceLabel = "External Transfer";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountId, AccountStatus = "Active" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await savingsService.DepositAsync(targetAccountId, depositAmount, fundingSourceLabel, requestingUserId);

            mockSavingsRepoProxy.Verify(proxy => proxy.DepositAsync(targetAccountId, depositAmount, fundingSourceLabel), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_RequestedAmountExceedsAccountBalance_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal excessiveWithdrawalAmount = 5000m;
            string withdrawalDestinationLabel = "Checking";
            int requestingUserId = 1;

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountId, AccountStatus = "Active", Balance = 1000m }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.WithdrawAsync(targetAccountId, excessiveWithdrawalAmount, withdrawalDestinationLabel, requestingUserId));
        }

        [Fact]
        public async Task WithdrawAsync_RequestedAmountIsNegative_ThrowsArgumentException()
        {
            int targetAccountId = 1;
            decimal negativeWithdrawalAmount = -5000m;
            string withdrawalDestinationLabel = "Checking";
            int requestingUserId = 1;

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.WithdrawAsync(targetAccountId, negativeWithdrawalAmount, withdrawalDestinationLabel, requestingUserId));
        }

        [Fact]
        public async Task WithdrawAsync_NoSavingsAccountWithGivenAccountIdFoundOnUserId_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal withdrawalAmount = 100m;
            int requestingUserId = 2;
            string withdrawalDestinationLabel = "Withdrawal";

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, false))
                .ReturnsAsync(new List<SavingsAccount>() { new SavingsAccount { IdentificationNumber = 0 } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.WithdrawAsync(targetAccountId, withdrawalAmount, withdrawalDestinationLabel, requestingUserId));
        }

        [Fact]
        public async Task WithdrawAsync_TargetAccountIsClosed_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal withdrawalAmount = 100m;
            int requestingUserId = 1;
            string withdrawalDestinationLabel = "Withdrawal";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountId, AccountStatus = "Closed" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.WithdrawAsync(targetAccountId, withdrawalAmount, withdrawalDestinationLabel, requestingUserId));
        }

        [Fact]
        public async Task WithdrawAsync_WithdrawalAmountBiggerThanBalance_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal withdrawalAmount = 100m;
            int requestingUserId = 1;
            string withdrawalDestinationLabel = "Withdrawal";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = targetAccountId, Balance = 50m }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.WithdrawAsync(targetAccountId, withdrawalAmount, withdrawalDestinationLabel, requestingUserId));
        }

        [Fact]
        public async Task WithdrawAsync_WithdrawalRequestFromFixedDepositBeforeAccountsMaturityDate_SetsEarlyWithdrawalPenalty()
        {
            int targetAccountId = 1;
            decimal withdrawalAmount = 100m;
            int requestingUserId = 1;
            string withdrawalDestinationLabel = "Withdrawal";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount
                {
                    IdentificationNumber = targetAccountId,
                    Balance = 200m,
                    SavingsType = "FixedDeposit",
                    MaturityDate = DateTime.UtcNow + new TimeSpan(1, 0, 0, 0),
                }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            decimal expectedEarlyWithdrawalPenalty = withdrawalAmount * DecimalEarlyWithdrawalPenalty;
            decimal expectedTotalSumToWithdraw = withdrawalAmount + expectedEarlyWithdrawalPenalty;

            await savingsService.WithdrawAsync(targetAccountId, withdrawalAmount, withdrawalDestinationLabel, requestingUserId);
            
            mockSavingsRepoProxy.Verify(proxy => proxy.WithdrawAsync(targetAccountId, expectedTotalSumToWithdraw, withdrawalDestinationLabel, expectedEarlyWithdrawalPenalty), Times.Once());
        }

        [Fact]
        public async Task WithdrawAsync_WithdrawalRequestFromFixedDepositWithPenaltyExceedsAccountsBalance_ThrowsInvalidOperationException()
        {
            int targetAccountId = 1;
            decimal withdrawalAmount = 100m;
            int requestingUserId = 1;
            string withdrawalDestinationLabel = "Withdrawal";

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount
                {
                    IdentificationNumber = targetAccountId,
                    Balance = 100m,
                    SavingsType = "FixedDeposit",
                    MaturityDate = DateTime.UtcNow + new TimeSpan(1, 0, 0, 0),
                }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.WithdrawAsync(targetAccountId, withdrawalAmount, withdrawalDestinationLabel, requestingUserId));
        }

        [Fact]
        public async Task CloseAccountAsync_DestinationAccountIsClosed_ThrowsInvalidOperationException()
        {
            int accountToCloseId = 1;
            int destinationAccountId = 2;
            int requestingUserId = 1;

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = accountToCloseId, AccountStatus = "Active" },
                new SavingsAccount { IdentificationNumber = destinationAccountId, AccountStatus = "Closed" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.CloseAccountAsync(accountToCloseId, destinationAccountId, requestingUserId));
        }

        [Fact]
        public async Task CloseAccountAsync_ClosingAccountIsAlreadyClosed_ThrowsInvalidOperationException()
        {
            int accountToCloseId = 1;
            int destinationAccountId = 2;
            int requestingUserId = 1;

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { IdentificationNumber = accountToCloseId, AccountStatus = "Closed" },
                new SavingsAccount { IdentificationNumber = destinationAccountId, AccountStatus = "Active" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            await Assert.ThrowsAsync<InvalidOperationException>(() => savingsService.CloseAccountAsync(accountToCloseId, destinationAccountId, requestingUserId));
        }

        [Fact]
        public async Task CloseAccountAsync_ClosingFixedDepositAccountBeforeItsMaturityDate_SetsEarlyClosurePenalty()
        {
            int accountToCloseId = 1;
            int destinationAccountId = 2;
            int requestingUserId = 1;

            List<SavingsAccount> userAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount
                {
                    IdentificationNumber = accountToCloseId,
                    AccountStatus = "Active",
                    SavingsType = "FixedDeposit",
                    MaturityDate = DateTime.UtcNow + new TimeSpan(1, 0, 0, 0),
                    Balance = 100m,
                },
                new SavingsAccount { IdentificationNumber = destinationAccountId, AccountStatus = "Active" }
            };

            mockSavingsRepoProxy.Setup(proxy => proxy.GetSavingsAccountsByUserIdAsync(requestingUserId, true))
                .ReturnsAsync(userAccountsList);

            decimal expectedEarlyClosurePenalty = userAccountsList[0].Balance * DecimalEarlyClosurePenalty;
            decimal expectedTransferAmount = userAccountsList[0].Balance - expectedEarlyClosurePenalty;

            await savingsService.CloseAccountAsync(accountToCloseId, destinationAccountId, requestingUserId);

            mockSavingsRepoProxy.Verify(proxy => proxy.CloseSavingsAccountAsync(accountToCloseId, destinationAccountId, expectedTransferAmount, expectedEarlyClosurePenalty), Times.Once);
        }

        [Fact]
        public async Task GetTransactionsAsync_PageIsNotPositiveInteger_ThrowsArgumentException()
        {
            int accountId = 1;
            string filterForTesting = "Test";
            int invalidPage = MinPage - 1;

            await Assert.ThrowsAsync<ArgumentException>(() => savingsService.GetTransactionsAsync(accountId, filterForTesting, invalidPage));
        }

        [Fact]
        public async Task GetTransactionsAsync_PageSizeLessThanOrEqualToZero_SetsPageSizeToDefaultPageSize()
        {
            int accountId = 1;
            string filterForTesting = "Test";
            int page = 1;
            int invalidPageSize = MinUserId - 1;

            await savingsService.GetTransactionsAsync(accountId, filterForTesting, page, invalidPageSize);

            mockSavingsRepoProxy.Verify(proxy => proxy.GetTransactionsAsync(accountId, filterForTesting, page, DefaultPageSize), Times.Once());
        }

        [Fact]
        public async Task GetTransactionsAsync_PageSizeMoreThanMaxPageSize_SetsPageSizeToDefaultPageSize()
        {
            int accountId = 1;
            string filterForTesting = "Test";
            int page = 1;
            int invalidPageSize = MaxPageSize + 1;

            await savingsService.GetTransactionsAsync(accountId, filterForTesting, page, invalidPageSize);

            mockSavingsRepoProxy.Verify(proxy => proxy.GetTransactionsAsync(accountId, filterForTesting, page, DefaultPageSize), Times.Once());
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_WantsToWithdrawFromFixedDepositBeforeMaturityDate_ReturnsTrue()
        {
            SavingsAccount fixedDepositAccount = new SavingsAccount
            {
                SavingsType = "FixedDeposit",
                MaturityDate = DateTime.UtcNow + new TimeSpan(1, 0, 0, 0),
            };

            bool riskIsPresent = await savingsService.HasRiskEarlyWithdrawal(fixedDepositAccount);

            Assert.True(riskIsPresent);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_WantsToWithdrawFromFixedDepositAfterMaturityDate_ReturnsFalse()
        {
            SavingsAccount fixedDepositAccount = new SavingsAccount
            {
                SavingsType = "FixedDeposit",
                MaturityDate = DateTime.UtcNow - new TimeSpan(1, 0, 0, 0),
            };

            bool riskIsPresent = await savingsService.HasRiskEarlyWithdrawal(fixedDepositAccount);

            Assert.False(riskIsPresent);
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