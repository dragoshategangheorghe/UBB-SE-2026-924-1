using System.Threading.Tasks;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Client.Services.Interfaces
{
    public interface ITransactionApiService
    {
        Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync();
        Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request);
        Task<TransactionDetailsResponse?> GetTransactionAsync(int transactionId);
        Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request);
        Task<ExportedFileResult?> ExportReceiptAsync(int transactionId);
    }

    public class ExportedFileResult
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
