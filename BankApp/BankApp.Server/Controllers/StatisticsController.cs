using Microsoft.AspNetCore.Mvc;
using BankApp.Server.Configuration;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using BankApp.Models.DTOs.Statistics;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ITransactionHistoryRepository transactionHistoryRepository;
        private readonly TeamCOptions options;

        public StatisticsController(ITransactionHistoryRepository transactionHistoryRepository, IOptions<TeamCOptions> options)
        {
            this.transactionHistoryRepository = transactionHistoryRepository;
            this.options = options.Value;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        [HttpGet("spending-by-category")]
        public IActionResult GetSpendingByCategory()
        {
            int userId = GetAuthenticatedUserId();
            List<TransactionHistoryItemDto> spendingTransactions = GetAnalyticsTransactions(userId)
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

            return Ok(new SpendingByCategoryResponse
            {
                Success = true,
                Message = "Spending by category loaded successfully.",
                TotalSpending = totalSpending,
                Categories = categories,
            });
        }

        [HttpGet("income-vs-expenses")]
        public IActionResult GetIncomeVsExpenses()
        {
            int userId = GetAuthenticatedUserId();
            List<TransactionHistoryItemDto> transactions = GetAnalyticsTransactions(userId);
            decimal income = transactions.Where(transaction => IsCredit(transaction.Direction)).Sum(transaction => transaction.Amount);
            decimal expenses = transactions.Where(transaction => IsDebit(transaction.Direction)).Sum(transaction => transaction.Amount);

            return Ok(new IncomeVsExpensesResponse
            {
                Success = true,
                Message = "Income and expenses loaded successfully.",
                Income = income,
                Expenses = expenses,
                Net = income - expenses,
            });
        }

        [HttpGet("balance-trends")]
        public IActionResult GetBalanceTrends()
        {
            int userId = GetAuthenticatedUserId();
            DateTime cutoffDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc);
            List<BalanceTrendPointDto> points = GetAnalyticsTransactions(userId)
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

            return Ok(new BalanceTrendsResponse
            {
                Success = true,
                Message = "Balance trends loaded successfully.",
                Points = points,
            });
        }

        [HttpGet("top-recipients")]
        public IActionResult GetTopRecipients()
        {
            int userId = GetAuthenticatedUserId();
            List<TopCounterpartyDto> recipients = GetAnalyticsTransactions(userId)
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

            return Ok(new TopRecipientsResponse
            {
                Success = true,
                Message = "Top recipients loaded successfully.",
                Recipients = recipients,
            });
        }

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

        private List<TransactionHistoryItemDto> GetAnalyticsTransactions(int userId)
        {
            return this.transactionHistoryRepository.GetTransactionsByUserId(userId)
                .Where(transaction => !IsFailed(transaction.Status))
                .ToList();
        }
    }
}
