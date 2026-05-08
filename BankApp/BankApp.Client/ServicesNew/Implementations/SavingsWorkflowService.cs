using System.Linq;
using System.Collections.Generic;
using BankApp.Models.Features.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.DTOs.Savings;

namespace BankApp.Server.Services.Implementations
{
    public class SavingsWorkflowService
    {
        private const int NoDestinationId = 0;
        private const decimal PositiveAmountThreshold = 0m;
        private const decimal NoPenaltyAmount = 0m;
        private const int FirstPage = 1;

        public FundingSourceOption? GetDefaultFundingSource(IEnumerable<FundingSourceOption> fundingSources)
        {
            return fundingSources.FirstOrDefault();
        }

        public int GetDefaultCloseDestinationId(IEnumerable<SavingsAccount> destinationAccounts)
        {
            return destinationAccounts.FirstOrDefault()?.IdentificationNumber ?? NoDestinationId;
        }

        public (bool IsValid, string ErrorMessage) ValidateWithdrawRequest(decimal amount, FundingSourceOption? destination)
        {
            if (amount <= PositiveAmountThreshold)
            {
                return (false, "Please enter a valid amount.");
            }

            if (destination == null)
            {
                return (false, "Please select a destination account.");
            }

            return (true, string.Empty);
        }

        public string BuildWithdrawResultMessage(WithdrawResponseDto response)
        {
            if (!response.Success)
            {
                return response.Message;
            }

            var penaltyText = response.PenaltyApplied > NoPenaltyAmount ? $" (penalty: ${response.PenaltyApplied:N2})" : string.Empty;
            return $"Withdrawn: ${response.AmountWithdrawn:N2}{penaltyText}. New balance: ${response.NewBalance:N2}";
        }

        public (bool IsValid, string ErrorMessage) ValidateCloseConfirmation(bool userConfirmed, int destinationId)
        {
            if (!userConfirmed)
            {
                return (false, "Please confirm account closure.");
            }

            if (destinationId == NoDestinationId)
            {
                return (false, "Please select a destination account.");
            }

            return (true, string.Empty);
        }

        public bool CanMoveToNextPage(int currentPage, int totalPages)
        {
            return currentPage < totalPages;
        }

        public bool CanMoveToPreviousPage(int currentPage)
        {
            return currentPage > FirstPage;
        }
    }
}