using BankApp.Models.Features.Investments;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface IInvestmentsService
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);

        /// <summary>
        /// Loads portfolio for <see cref="IAuthService.GetCurrentUserId"/>; returns null when not signed in.
        /// </summary>
        Task<Portfolio?> GetPortfolioForCurrentUserAsync();
    }
}

