using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface ISavingsWorkflowRepoProxy
    {
        Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources);

        Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts);

        Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination);

        Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response);

        Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId);

        Task<bool> CanMoveToNextPage(int currentPage, int totalPages);

        Task<bool> CanMoveToPreviousPage(int currentPage);
    }
}
