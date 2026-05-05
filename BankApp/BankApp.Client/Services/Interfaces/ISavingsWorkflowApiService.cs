using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using Microsoft.AspNetCore.Mvc;
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

        Task<ActionResult> ValidateWithdrawRequest(ValidateWithdrawRequestDto request);

        Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response);

        Task<ActionResult> ValidateCloseConfirmation(bool userConfirmed, int destinationId);

        Task<bool> CanMoveToNextPage(int currentPage, int totalPages);

        Task<bool> CanMoveToPreviousPage(int currentPage);
    }
}
