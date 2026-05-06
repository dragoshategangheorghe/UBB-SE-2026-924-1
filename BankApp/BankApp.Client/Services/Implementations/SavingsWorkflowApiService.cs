using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class SavingsWorkflowApiService : ISavingsWorkflowApiService
    {
        private readonly ApiService _apiService;

        public SavingsWorkflowApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<string> BuildWithdrawResultMessage(WithdrawResponseDto response)
        {
            return await _apiService.PostAsync<WithdrawResponseDto, string>("api/savings-workflow/withdraw-result-message", response);
        }

        public async Task<bool> CanMoveToNextPage(int currentPage, int totalPages)
        {
            return await _apiService.GetAsync<bool>($"api/savings-workflow/can-move-next?currentPage={currentPage}&totalPages={totalPages}");
        }

        public async Task<bool> CanMoveToPreviousPage(int currentPage)
        {
            return await _apiService.GetAsync<bool>($"api/savings-workflow/can-move-previous?currentPage={currentPage}");
        }

        public async Task<int> GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts)
        {
            return await _apiService.PostAsync<IEnumerable<SavingsAccount>, int>("api/savings-workflow/default-close-destination", destinationAccounts);
        }

        public async Task<FundingSourceOption> GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources)
        {
            return await _apiService.PostAsync<IEnumerable<FundingSourceOption>, FundingSourceOption>("api/savings-workflow/default-funding-source", fundingSources);
        }

        public async Task<ValidationResponse> ValidateCloseConfirmation(bool userConfirmed, int destinationId)
        {
            return await _apiService.GetAsync<ValidationResponse>($"api/savings-workflow/validate-close?userConfirmed={userConfirmed}&destinationId={destinationId}");
        }

        public async Task<ValidationResponse> ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination)
        {
            return await _apiService.PostAsync<FundingSourceOption?, ValidationResponse>($"api/savings-workflow?amount={amount}", destination);
        }
    }
}
