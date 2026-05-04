using BankApp.Models.DTOs.Transactions;

namespace BankApp.Server.Services.Interfaces
{
    public interface ITransactionHistoryService
    {
        TransactionHistoryResponse GetHistory(int userId, TransactionHistoryRequest request);
        TransactionDetailsResponse GetTransaction(int userId, int transactionId);
        TransactionFilterMetadataResponse GetFilterMetadata(int userId);
        TransactionExportResult ExportTransactions(int userId, TransactionExportRequest request);
        TransactionExportResult ExportReceipt(int userId, int transactionId);
    }
}
