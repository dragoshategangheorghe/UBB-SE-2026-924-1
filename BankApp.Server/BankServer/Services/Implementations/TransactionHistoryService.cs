using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;

namespace BankApp.Server.Services.Implementations
{
    public class TransactionHistoryService : ITransactionHistoryService
    {
        private readonly ITransactionHistoryRepository transactionHistoryRepository;
        private readonly ITransactionExportService transactionExportService;

        public TransactionHistoryService(ITransactionHistoryRepository transactionHistoryRepository, ITransactionExportService transactionExportService)
        {
            this.transactionHistoryRepository = transactionHistoryRepository;
            this.transactionExportService = transactionExportService;
        }

        public TransactionHistoryResponse GetHistory(int userId, TransactionHistoryRequest request)
        {
            TransactionHistoryRequest normalizedRequest = NormalizeRequest(request);
            List<TransactionHistoryItemDto> transactions = transactionHistoryRepository.GetTransactionsByUserId(userId);
            List<TransactionHistoryItemDto> filteredTransactions = ApplyFiltersAndSort(transactions, normalizedRequest);

            return new TransactionHistoryResponse
            {
                Success = true,
                Message = "Transaction history loaded successfully.",
                AppliedFilters = normalizedRequest,
                Transactions = filteredTransactions
            };
        }

        public TransactionDetailsResponse GetTransaction(int userId, int transactionId)
        {
            TransactionHistoryItemDto? transaction = transactionHistoryRepository.GetTransactionById(userId, transactionId);
            if (transaction == null)
            {
                return new TransactionDetailsResponse
                {
                    Success = false,
                    Message = "Transaction not found."
                };
            }

            return new TransactionDetailsResponse
            {
                Success = true,
                Message = "Transaction details loaded successfully.",
                Transaction = transaction
            };
        }

        public TransactionFilterMetadataResponse GetFilterMetadata(int userId)
        {
            List<TransactionHistoryItemDto> transactions = transactionHistoryRepository.GetTransactionsByUserId(userId);

            return new TransactionFilterMetadataResponse
            {
                Success = true,
                Message = "Transaction filters loaded successfully.",
                Accounts = transactionHistoryRepository.GetAccountsByUserId(userId)
                    .Select(account => new AccountFilterOptionDto
                    {
                        Id = account.Id,
                        Name = account.AccountName ?? $"Account {account.Id}",
                        Iban = account.IBAN
                    })
                    .OrderBy(account => account.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Cards = transactionHistoryRepository.GetCardsByUserId(userId)
                    .Select(card => new CardFilterOptionDto
                    {
                        Id = card.Id,
                        Label = $"{(card.CardBrand ?? card.CardType)} {MaskCardNumber(card.CardNumber)}"
                    })
                    .OrderBy(card => card.Label, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                AvailableTransactionTypes = transactions
                    .Select(transaction => transaction.TransactionType)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                AvailableStatuses = transactions
                    .Select(transaction => transaction.Status)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                AvailableDirections = transactions
                    .Select(transaction => transaction.Direction)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }

        public TransactionExportResult ExportTransactions(int userId, TransactionExportRequest request)
        {
            TransactionHistoryResponse history = GetHistory(userId, request);
            return transactionExportService.ExportStatement(history.Transactions, history.AppliedFilters, request.Format);
        }

        public TransactionExportResult ExportReceipt(int userId, int transactionId)
        {
            TransactionDetailsResponse response = GetTransaction(userId, transactionId);
            if (!response.Success || response.Transaction == null)
            {
                return new TransactionExportResult();
            }

            return transactionExportService.ExportReceipt(response.Transaction);
        }

        internal List<TransactionHistoryItemDto> ApplyFiltersAndSort(IEnumerable<TransactionHistoryItemDto> transactions, TransactionHistoryRequest request)
        {
            IEnumerable<TransactionHistoryItemDto> query = transactions;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(transaction =>
                    ContainsInsensitive(transaction.CounterpartyOrMerchant, request.SearchTerm) ||
                    ContainsInsensitive(transaction.ReferenceNumber, request.SearchTerm) ||
                    ContainsInsensitive(transaction.Description, request.SearchTerm));
            }

            if (request.FromDate.HasValue)
            {
                DateTime fromDate = request.FromDate.Value.Date;
                query = query.Where(transaction => transaction.Timestamp.Date >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                DateTime toDate = request.ToDate.Value.Date;
                query = query.Where(transaction => transaction.Timestamp.Date <= toDate);
            }

            if (!string.IsNullOrWhiteSpace(request.TransactionType))
            {
                query = query.Where(transaction => string.Equals(transaction.TransactionType, request.TransactionType, StringComparison.OrdinalIgnoreCase));
            }

            if (request.MinimumAmount.HasValue)
            {
                query = query.Where(transaction => transaction.Amount >= request.MinimumAmount.Value);
            }

            if (request.MaximumAmount.HasValue)
            {
                query = query.Where(transaction => transaction.Amount <= request.MaximumAmount.Value);
            }

            if (request.AccountId.HasValue)
            {
                query = query.Where(transaction => transaction.AccountId == request.AccountId.Value);
            }

            if (request.CardId.HasValue)
            {
                query = query.Where(transaction => transaction.CardId == request.CardId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(transaction => string.Equals(transaction.Status, request.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.Direction))
            {
                query = query.Where(transaction => string.Equals(transaction.Direction, request.Direction, StringComparison.OrdinalIgnoreCase));
            }

            bool sortAscending = string.Equals(request.SortDirection, SortDirections.Asc, StringComparison.OrdinalIgnoreCase);
            query = string.Equals(request.SortField, TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase)
                ? (sortAscending ? query.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Timestamp) : query.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Timestamp))
                : (sortAscending ? query.OrderBy(transaction => transaction.Timestamp).ThenBy(transaction => transaction.Id) : query.OrderByDescending(transaction => transaction.Timestamp).ThenByDescending(transaction => transaction.Id));

            return query.ToList();
        }

        private static TransactionHistoryRequest NormalizeRequest(TransactionHistoryRequest request)
        {
            return new TransactionHistoryRequest
            {
                SearchTerm = request.SearchTerm?.Trim(),
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TransactionType = NormalizeOptionalValue(request.TransactionType),
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                AccountId = request.AccountId,
                CardId = request.CardId,
                Status = NormalizeOptionalValue(request.Status),
                Direction = NormalizeOptionalValue(request.Direction),
                SortField = NormalizeSortField(request.SortField),
                SortDirection = NormalizeSortDirection(request.SortDirection)
            };
        }

        private static string NormalizeSortField(string? sortField)
        {
            return string.Equals(sortField, TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase)
                ? TransactionSortFields.Amount
                : TransactionSortFields.Date;
        }

        private static string NormalizeSortDirection(string? sortDirection)
        {
            return string.Equals(sortDirection, SortDirections.Asc, StringComparison.OrdinalIgnoreCase)
                ? SortDirections.Asc
                : SortDirections.Desc;
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool ContainsInsensitive(string? source, string searchTerm)
        {
            return !string.IsNullOrWhiteSpace(source) &&
                   source.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            {
                return "****";
            }

            return $"**** {cardNumber[^4..]}";
        }
    }
}
