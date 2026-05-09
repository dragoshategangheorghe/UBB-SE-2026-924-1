using System.Threading.Tasks;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface ITransactionApiService
    {
        Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync();

        Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request);

        Task<TransactionDetailsResponse?> GetTransactionAsync(int transactionId);

        Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request);

        Task<ExportedFileResult?> ExportReceiptAsync(int transactionId);
    }
}
