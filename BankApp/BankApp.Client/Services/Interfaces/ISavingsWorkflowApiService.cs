using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsWorkflowApiService
    {
        Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources);

        Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts);

        Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination);

        Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response);

        Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId);

        Task<bool> CanMoveToNextPage(int currentPage, int totalPages);

        Task<bool> CanMoveToPreviousPage(int currentPage);
    }

    public class ValidationResponse
    {
        public bool IsValid {  get; set; }
        public string ErrorMessage { get; set; }
    }
}
