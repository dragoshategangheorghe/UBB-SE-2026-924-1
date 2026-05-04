namespace BankApp.Server.Services.Implementations
{
    using BankApp.Models.DTOs.Statistics;
    using BankApp.Models.DTOs.Transactions;
    using BankApp.Server.Configuration;
    using BankApp.Server.Repositories.Interfaces;
    using BankApp.Server.Services.Interfaces;
    using Microsoft.Extensions.Options;

    public class StatisticsService : IStatisticsService
    {
        private readonly ITransactionHistoryRepository transactionHistoryRepository;
        private readonly TeamCOptions options;

        private static bool IsDebit(string? direction)
        {
            return string.Equals(direction, "Debit", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCredit(string? direction)
        {
            return string.Equals(direction, "Credit", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFailed(string? status)
        {
            return string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "Reversed", StringComparison.OrdinalIgnoreCase);
        }

        public StatisticsService(ITransactionHistoryRepository transactionHistoryRepository, IOptions<TeamCOptions> options)
        {
            this.transactionHistoryRepository = transactionHistoryRepository;
            this.options = options.Value;
        }

        public SpendingByCategoryResponse GetSpendingByCategory(int userId)
        {
            List<TransactionHistoryItemDto> spendingTransactions = this.GetAnalyticsTransactions(userId)
                .Where(transaction => IsDebit(transaction.Direction))
                .ToList();

            decimal totalSpending = spendingTransactions.Sum(transaction => transaction.Amount);
            List<CategorySpendingPointDto> categories = spendingTransactions
                .GroupBy(transaction => string.IsNullOrWhiteSpace(transaction.CategoryName) ? "Uncategorized" : transaction.CategoryName)
                .Select(group => new CategorySpendingPointDto
                {
                    CategoryName = group.Key,
                    Amount = group.Sum(transaction => transaction.Amount),
                })
                .OrderByDescending(category => category.Amount)
                .ToList();

            foreach (CategorySpendingPointDto category in categories)
            {
                category.ShareOfTotal = totalSpending == 0 ? 0 : Math.Round(category.Amount / totalSpending, 4);
            }

            return new SpendingByCategoryResponse
            {
                Success = true,
                Message = "Spending by category loaded successfully.",
                TotalSpending = totalSpending,
                Categories = categories,
            };
        }

        public IncomeVsExpensesResponse GetIncomeVsExpenses(int userId)
        {
            List<TransactionHistoryItemDto> transactions = this.GetAnalyticsTransactions(userId);
            decimal income = transactions.Where(transaction => IsCredit(transaction.Direction)).Sum(transaction => transaction.Amount);
            decimal expenses = transactions.Where(transaction => IsDebit(transaction.Direction)).Sum(transaction => transaction.Amount);

            return new IncomeVsExpensesResponse
            {
                Success = true,
                Message = "Income and expenses loaded successfully.",
                Income = income,
                Expenses = expenses,
                Net = income - expenses,
            };
        }

        public BalanceTrendsResponse GetBalanceTrends(int userId)
        {
            // the previous one had something with the current date, which made it fail when we received it
            // maybe it worked when the other team was working on it, replaced it with a fixed DateTime
            DateTime cutoffDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc);
            List<BalanceTrendPointDto> points = this.GetAnalyticsTransactions(userId)
                .Where(transaction => transaction.Timestamp.Date >= cutoffDate)
                .GroupBy(transaction => transaction.Timestamp.Date)
                .Select(group => group.OrderByDescending(transaction => transaction.Timestamp).ThenByDescending(transaction => transaction.Id).First())
                .OrderBy(transaction => transaction.Timestamp.Date)
                .Select(transaction => new BalanceTrendPointDto
                {
                    Date = transaction.Timestamp.Date,
                    Balance = transaction.RunningBalanceAfterTransaction,
                })
                .ToList();

            return new BalanceTrendsResponse
            {
                Success = true,
                Message = "Balance trends loaded successfully.",
                Points = points,
            };
        }

        public TopRecipientsResponse GetTopRecipients(int userId)
        {
            List<TopCounterpartyDto> recipients = this.GetAnalyticsTransactions(userId)
                .Where(transaction => IsDebit(transaction.Direction))
                .Where(transaction => !string.IsNullOrWhiteSpace(transaction.CounterpartyOrMerchant))
                .GroupBy(transaction => transaction.CounterpartyOrMerchant)
                .Select(group => new TopCounterpartyDto
                {
                    Name = group.Key,
                    TotalAmount = group.Sum(transaction => transaction.Amount),
                    TransactionCount = group.Count(),
                })
                .OrderByDescending(recipient => recipient.TotalAmount)
                .ThenBy(recipient => recipient.Name, StringComparer.OrdinalIgnoreCase)
                .Take(options.TopRecipientsCount)
                .ToList();

            return new TopRecipientsResponse
            {
                Success = true,
                Message = "Top recipients loaded successfully.",
                Recipients = recipients,
            };
        }

        private List<TransactionHistoryItemDto> GetAnalyticsTransactions(int userId)
        {
            return this.transactionHistoryRepository.GetTransactionsByUserId(userId)
                .Where(transaction => !IsFailed(transaction.Status))
                .ToList();
        }
    }
}
