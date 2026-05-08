using BankApp.Models.DTOs.Statistics;

namespace BankApp.Server.Services.Interfaces
{
    public interface IStatisticsService
    {
        SpendingByCategoryResponse GetSpendingByCategory(int userId);
        IncomeVsExpensesResponse GetIncomeVsExpenses(int userId);
        BalanceTrendsResponse GetBalanceTrends(int userId);
        TopRecipientsResponse GetTopRecipients(int userId);
    }
}
