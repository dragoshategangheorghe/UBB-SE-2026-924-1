using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Client.State
{
    public class TransactionHistorySessionState : ITransactionHistorySessionState
    {
        public TransactionHistoryRequest? CurrentRequest { get; set; }

        public int? SelectedTransactionId { get; set; }

        public void Clear()
        {
            CurrentRequest = null;
            SelectedTransactionId = null;
        }
    }
}
