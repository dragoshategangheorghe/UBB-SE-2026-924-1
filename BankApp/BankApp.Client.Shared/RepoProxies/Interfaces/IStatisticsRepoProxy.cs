using System.Threading.Tasks;
using BankApp.Models.DTOs.Statistics;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IStatisticsRepoProxy
    {
        Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync();

        Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync();

        Task<BalanceTrendsResponse?> GetBalanceTrendsAsync();

        Task<TopRecipientsResponse?> GetTopRecipientsAsync();
    }
}
