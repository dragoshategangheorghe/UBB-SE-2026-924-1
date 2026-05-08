using BankApp.Models.DTOs.Transactions;

namespace BankApp.Server.Services.Interfaces
{
    public interface ITransactionExportService
    {
        TransactionExportResult ExportStatement(IReadOnlyCollection<TransactionHistoryItemDto> transactions, TransactionHistoryRequest request, string format);
        TransactionExportResult ExportReceipt(TransactionHistoryItemDto transaction);
    }
}
