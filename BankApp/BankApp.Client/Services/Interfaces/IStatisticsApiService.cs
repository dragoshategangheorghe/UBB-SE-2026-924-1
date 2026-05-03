using System.Threading.Tasks;
using BankApp.Models.DTOs.Statistics;

namespace BankApp.Client.Services.Interfaces
{
    public interface IStatisticsApiService
    {
        Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();
        Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();
        Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();
        Task<TopRecipientsResponse?> GetTopRecipientsAsync();
    }
}
