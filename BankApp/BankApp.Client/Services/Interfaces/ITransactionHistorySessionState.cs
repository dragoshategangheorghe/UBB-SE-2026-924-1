using BankApp.Models.DTOs.Transactions;

namespace BankApp.Client.Services.Interfaces
{
    public interface ITransactionHistorySessionState
    {
        TransactionHistoryRequest? CurrentRequest { get; set; }
        int? SelectedTransactionId { get; set; }
        void Clear();
    }
}
