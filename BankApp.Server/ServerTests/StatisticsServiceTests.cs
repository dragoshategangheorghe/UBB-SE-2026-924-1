using BankApp.Models.DTOs.Statistics;
using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Configuration;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using NUnit.Framework;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class StatisticsServiceTests
    {
        private ITransactionHistoryRepository mockTransactionHistoryRepository;
        private IOptions<TeamCOptions> options;
        private StatisticsService statisticsService;

        private static int GetTestId()
        {
            return 1;
        }

        private static List<TransactionHistoryItemDto> CreateTransactionsForGetSpendingByCategory()
        {
            return new List<TransactionHistoryItemDto>
            {
                new () { Amount = 134, Direction = "Debit", Status = "Completed", CategoryName = "Food" },
                new () { Amount = 70, Direction = "Credit", Status = "Completed", CategoryName = "Food" },
                new () { Amount = 66, Direction = "Debit", Status = "Completed", CategoryName = "Transport" },
                new () { Amount = 90, Direction = "Credit", Status = "Completed", CategoryName = "Purchase" },
            };
        }

        private static List<TransactionHistoryItemDto> CreateTransactionsWithEmptyCategoryAndZeroAmount()
        {
            return new List<TransactionHistoryItemDto>
            {
                new () { Amount = 0, Direction = "Debit", Status = "Completed", CategoryName = " " },
                new () { Amount = 0, Direction = "Debit", Status = "Completed", CategoryName = "    " },
            };
        }

        private static List<TransactionHistoryItemDto> CreateCreditAndDebitTransactions()
        {
            return new List<TransactionHistoryItemDto>
            {
                new () { Amount = 94, Direction = "Credit", Status = "Completed" },
                new () { Amount = 53, Direction = "Debit", Status = "Completed" },
                new () { Amount = 138, Direction = "Debit", Status = "Failed" },
                new () { Amount = 13, Direction = "Credit", Status = "Completed" },
                new () { Amount = 32, Direction = "Debit", Status = "Completed" },
            };
        }

        private static List<TransactionHistoryItemDto> CreateTransactionsForGetTopRecipients()
        {
            return new List<TransactionHistoryItemDto>
            {
                new () { Amount = 100, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "A" },
                new () { Amount = 110, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "B" },
                new () { Amount = 50, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "C" },
                new () { Amount = 32, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "D" },
                new () { Amount = 32, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "D" },
                new () { Amount = 102, Direction = "Debit", Status = "Failed", CounterpartyOrMerchant = "E" },
                new () { Amount = 93, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "E" },
                new () { Amount = 120, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "F" },
            };
        }

        private static List<TransactionHistoryItemDto> CreateTransactionsForOnlyOneRecipient()
        {
            return new List<TransactionHistoryItemDto>
            {
                new () { Amount = 102, Direction = "Debit", Status = "Failed", CounterpartyOrMerchant = "E" },
                new () { Amount = 53, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "E" },
                new () { Amount = 93, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = "E" },
            };
        }

        [SetUp]
        public void SetUp()
        {
            mockTransactionHistoryRepository = Substitute.For<ITransactionHistoryRepository>();
            options = Options.Create(new TeamCOptions());

            statisticsService = new StatisticsService(mockTransactionHistoryRepository, options);
        }

        [Test]
        public void GetSpendingByCategory_SuccessfulDebitTransactionsPresent_SuccessResponse()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsForGetSpendingByCategory());

            var result = statisticsService.GetSpendingByCategory(GetTestId());

            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void GetSpendingByCategory_SuccessfulDebitTransactionsPresent_CorrectTotalSpendingCalculation()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsForGetSpendingByCategory());

            var result = statisticsService.GetSpendingByCategory(GetTestId());

            Assert.That(result.TotalSpending, Is.EqualTo(200));
        }

        [Test]
        public void GetSpendingByCategory_SuccessfulDebitTransactionsPresent_CorrectCategorySpendingPoints()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsForGetSpendingByCategory());
            List<CategorySpendingPointDto> expectedCategorySpendingPoints = new List<CategorySpendingPointDto>
            {
                new () { CategoryName = "Food", Amount = 134, ShareOfTotal = 0.67m },
                new () { CategoryName = "Transport", Amount = 66, ShareOfTotal = 0.33m },
            };

            var result = statisticsService.GetSpendingByCategory(GetTestId());

            Assert.That(result.Categories, Is.EqualTo(expectedCategorySpendingPoints));
        }

        [Test]
        public void GetSpendingByCategory_TransactionsWithNoCategoryPresent_TransactionCategorizedAsUncategorized()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsWithEmptyCategoryAndZeroAmount());

            SpendingByCategoryResponse result = statisticsService.GetSpendingByCategory(GetTestId());

            Assert.That(result.Categories[0].CategoryName, Is.EqualTo("Uncategorized"));
        }

        [Test]
        public void GetSpendingByCategory_AllTransactionsWithZeroAmount_AllSharesOfTotalSpendingByCategoryEqualToZero()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsWithEmptyCategoryAndZeroAmount());

            SpendingByCategoryResponse result = statisticsService.GetSpendingByCategory(GetTestId());

            Assert.That(result.Categories[0].ShareOfTotal, Is.Zero);
        }

        [Test]
        public void GetIncomeVsExpenses_SuccessfulTransactionsPresent_ComputesIncome()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateCreditAndDebitTransactions());

            IncomeVsExpensesResponse result = statisticsService.GetIncomeVsExpenses(GetTestId());

            Assert.That(result.Income, Is.EqualTo(107));
        }

        [Test]
        public void GetIncomeVsExpenses_SuccessfulTransactionsPresent_ComputesExpenses()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateCreditAndDebitTransactions());

            IncomeVsExpensesResponse result = statisticsService.GetIncomeVsExpenses(GetTestId());

            Assert.That(result.Expenses, Is.EqualTo(85));
        }

        [Test]
        public void GetIncomeVsExpenses_SuccessfulTransactionsPresent_ComputesNet()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateCreditAndDebitTransactions());

            IncomeVsExpensesResponse result = statisticsService.GetIncomeVsExpenses(GetTestId());

            Assert.That(result.Net, Is.EqualTo(22));
        }

        [Test]
        public void GetBalanceTrends_SuccessfulTransactionsPresentAcrossMultipleDays_SelectsLatestBalanceEveryDay()
        {
            var cutoff = new DateTime(2026, 3, 24);
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(new List<TransactionHistoryItemDto>
            {
                new TransactionHistoryItemDto { Id = 1, Timestamp = cutoff.AddHours(3), RunningBalanceAfterTransaction = 100, Status = "Completed" },
                new TransactionHistoryItemDto { Id = 2, Timestamp = cutoff.AddHours(2), RunningBalanceAfterTransaction = 200, Status = "Completed" },
                new TransactionHistoryItemDto { Id = 3, Timestamp = cutoff.AddDays(1), RunningBalanceAfterTransaction = 300, Status = "Completed" }
            });

            BalanceTrendsResponse result = statisticsService.GetBalanceTrends(GetTestId());
            var expectedResult = new BalanceTrendsResponse
            {
                Success = true,
                Message = "Balance trends loaded successfully.",
                Points = new List<BalanceTrendPointDto>
                {
                    new () { Date = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc), Balance = 100 },
                    new () { Date = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc), Balance = 300 },
                }
            };
            Assert.That(result.Points.SequenceEqual(expectedResult.Points), Is.EqualTo(true));
        }

        [Test]
        public void GetTopRecipients_SuccessfulDebitTransactionsPresent_ReturnsTopFiveRecipientsOrderedByTotalAmountReceivedDescending()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsForGetTopRecipients());

            TopRecipientsResponse result = statisticsService.GetTopRecipients(GetTestId());

            Assert.That(result.Recipients.Count, Is.EqualTo(5));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Recipients[0].Name, Is.EqualTo("F"));
                Assert.That(result.Recipients[1].Name, Is.EqualTo("B"));
                Assert.That(result.Recipients[2].Name, Is.EqualTo("A"));
                Assert.That(result.Recipients[3].Name, Is.EqualTo("E"));
                Assert.That(result.Recipients[4].Name, Is.EqualTo("D"));
            }
        }

        [Test]
        public void GetTopRecipients_SuccessfulDebitTransactionsPresentWithOnlyOneRecipient_ReturnsNumberOfTransactions()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsForOnlyOneRecipient());

            TopRecipientsResponse result = statisticsService.GetTopRecipients(GetTestId());

            Assert.That(result.Recipients[0].TransactionCount, Is.EqualTo(2));
        }

        [Test]
        public void GetTopRecipients_SuccessfulDebitTransactionsPresentWithOnlyOneRecipient_ReturnsTotalAmount()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(CreateTransactionsForOnlyOneRecipient());

            TopRecipientsResponse result = statisticsService.GetTopRecipients(GetTestId());

            Assert.That(result.Recipients[0].TotalAmount, Is.EqualTo(146));
        }

        [Test]
        public void GetTopRecipients_OnlyTransactionsPresentWithMissingRecipients_ReturnsEmptyListOfTopRecipients()
        {
            mockTransactionHistoryRepository.GetTransactionsByUserId(GetTestId()).Returns(new List<TransactionHistoryItemDto>
            {
                new () { Amount = 123, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = " " },
                new () { Amount = 345, Direction = "Debit", Status = "Completed", CounterpartyOrMerchant = " " },
            });

            TopRecipientsResponse result = statisticsService.GetTopRecipients(GetTestId());

            Assert.That(result.Recipients, Is.Empty);
        }
    }
}
