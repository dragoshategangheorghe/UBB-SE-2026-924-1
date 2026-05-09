using BankApp.Models.DTOs.Statistics;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();
        Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();
        Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();
        Task<TopRecipientsResponse?> GetTopRecipientsAsync();
    }
}

