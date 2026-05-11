using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Client.Services.Implementations
{
    public class TransactionHistoryService : ITransactionHistoryService
    {
        private readonly ITransactionRepoProxy _repoProxy;

        public TransactionHistoryService(ITransactionRepoProxy repoProxy)
        {
            _repoProxy = repoProxy;
        }

        public Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync() => _repoProxy.GetFilterMetadataAsync();
        public Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request) => _repoProxy.GetHistoryAsync(request);
        public Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request) => _repoProxy.ExportTransactionsAsync(request);
        public Task<ExportedFileResult?> ExportReceiptAsync(int transactionId) => _repoProxy.ExportReceiptAsync(transactionId);
    }
}

