using BankApp.Models.DTOs.Transactions;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ITransactionHistoryService
    {
        Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync();
        Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request);
        Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request);
        Task<ExportedFileResult?> ExportReceiptAsync(int transactionId);
    }
}

