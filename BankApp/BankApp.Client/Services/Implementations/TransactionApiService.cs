using System;
using System.IO;
using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Client.Services.Implementations
{
    public class TransactionApiService : ITransactionApiService
    {
        private readonly ApiService _apiService;

        public TransactionApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<TransactionFilterMetadataResponse?> GetFilterMetadataAsync()
        {
            return _apiService.GetAsync<TransactionFilterMetadataResponse>("api/transactions/filters");
        }

        public Task<TransactionHistoryResponse?> GetHistoryAsync(TransactionHistoryRequest request)
        {
            return _apiService.PostAsync<TransactionHistoryRequest, TransactionHistoryResponse>("api/transactions/history", request);
        }

        public Task<TransactionDetailsResponse?> GetTransactionAsync(int transactionId)
        {
            return _apiService.GetAsync<TransactionDetailsResponse>($"api/transactions/{transactionId}");
        }

        public async Task<ExportedFileResult?> ExportTransactionsAsync(TransactionExportRequest request)
        {
            DownloadResponse? response = await _apiService.PostDownloadAsync("api/transactions/export", request);
            return response == null ? null : await SaveDownloadAsync(response);
        }

        public async Task<ExportedFileResult?> ExportReceiptAsync(int transactionId)
        {
            DownloadResponse? response = await _apiService.GetDownloadAsync($"api/transactions/{transactionId}/receipt");
            return response == null ? null : await SaveDownloadAsync(response);
        }

        private static async Task<ExportedFileResult> SaveDownloadAsync(DownloadResponse response)
        {
            string exportDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BankAppExports");

            Directory.CreateDirectory(exportDirectory);

            string fileName = string.IsNullOrWhiteSpace(response.FileName) ? "download.bin" : response.FileName;
            string filePath = Path.Combine(exportDirectory, fileName);

            if (File.Exists(filePath))
            {
                string timestampedName = $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
                filePath = Path.Combine(exportDirectory, timestampedName);
            }

            await File.WriteAllBytesAsync(filePath, response.Content);

            return new ExportedFileResult
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                ContentType = response.ContentType
            };
        }
    }
}
