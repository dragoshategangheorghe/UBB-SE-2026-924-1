using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Transactions;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class TransactionHistoryService : ITransactionHistoryService
    {
        private readonly ITransactionApiService _repoProxy;

        public TransactionHistoryService(ITransactionApiService repoProxy)
        {
            _repoProxy = repoProxy;
        }

        public Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync() => _repoProxy.GetFilterMetadataAsync();
        public Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request) => _repoProxy.GetHistoryAsync(request);
        public Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request) => _repoProxy.ExportTransactionsAsync(request);
        public Task<ExportedFileResult?> ExportReceiptAsync(int transactionId) => _repoProxy.ExportReceiptAsync(transactionId);
    }
}

