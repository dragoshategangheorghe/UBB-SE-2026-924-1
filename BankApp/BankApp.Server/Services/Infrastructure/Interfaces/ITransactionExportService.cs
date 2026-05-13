using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Services.Infrastructure;

namespace BankApp.Server.Services.Infrastructure.Interfaces
{
    public interface ITransactionExportService
    {
        TransactionExportResult ExportStatement(IReadOnlyCollection<TransactionHistoryItemDto> transactions, TransactionHistoryRequest request, string format);
        TransactionExportResult ExportReceipt(TransactionHistoryItemDto transaction);
    }
}
