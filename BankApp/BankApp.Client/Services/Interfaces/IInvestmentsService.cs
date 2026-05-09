using BankApp.Models.Features.Investments;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface IInvestmentsService
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);
    }
}

