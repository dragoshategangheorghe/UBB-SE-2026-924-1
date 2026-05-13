namespace BankApp.Client.Services.Interfaces
{
    using System.Threading.Tasks;
    using BankApp.Models.Entities;

    public interface IInvestmentsService
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);
        Task<Portfolio?> GetPortfolioForCurrentUserAsync();
        Task<bool> ExecuteTradeAsync(int userId, string ticker, string action, decimal quantity, decimal price);
    }
}