using System;
using System.Collections.Generic;
using System.Linq;
using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionHistoryRepository _transactionHistoryRepository;
        private readonly ITransactionExportService _transactionExportService;

        public TransactionsController(ITransactionHistoryRepository transactionHistoryRepository, ITransactionExportService transactionExportService)
        {
            _transactionHistoryRepository = transactionHistoryRepository;
            _transactionExportService = transactionExportService;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        [HttpGet("filters")]
        public IActionResult GetFilterMetadata()
        {
            int userId = GetAuthenticatedUserId();
            List<TransactionHistoryItemDto> transactions = _transactionHistoryRepository.GetTransactionsByUserId(userId);

            return Ok(new TransactionFilterMetadataResponse
            {
                Success = true,
                Message = "Transaction filters loaded successfully.",
                Accounts = _transactionHistoryRepository.GetAccountsByUserId(userId)
                    .Select(account => new AccountFilterOptionDto
                    {
                        Id = account.Id,
                        Name = account.AccountName ?? $"Account {account.Id}",
                        Iban = account.IBAN
                    })
                    .OrderBy(account => account.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Cards = _transactionHistoryRepository.GetCardsByUserId(userId)
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
            });
        }

        [HttpPost("history")]
        public IActionResult GetHistory([FromBody] TransactionHistoryRequest request)
        {
            int userId = GetAuthenticatedUserId();
            TransactionHistoryRequest normalizedRequest = NormalizeRequest(request);
            List<TransactionHistoryItemDto> transactions = _transactionHistoryRepository.GetTransactionsByUserId(userId);
            List<TransactionHistoryItemDto> filteredTransactions = ApplyFiltersAndSort(transactions, normalizedRequest);

            return Ok(new TransactionHistoryResponse
            {
                Success = true,
                Message = "Transaction history loaded successfully.",
                AppliedFilters = normalizedRequest,
                Transactions = filteredTransactions
            });
        }

        [HttpGet("{transactionId:int}")]
        public IActionResult GetTransaction(int transactionId)
        {
            int userId = GetAuthenticatedUserId();
            TransactionHistoryItemDto? transaction = _transactionHistoryRepository.GetTransactionById(userId, transactionId);
            TransactionDetailsResponse response = transaction == null
                ? new TransactionDetailsResponse { Success = false, Message = "Transaction not found." }
                : new TransactionDetailsResponse { Success = true, Message = "Transaction details loaded successfully.", Transaction = transaction };
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost("export")]
        public IActionResult ExportTransactions([FromBody] TransactionExportRequest request)
        {
            int userId = GetAuthenticatedUserId();
            TransactionHistoryRequest normalizedRequest = NormalizeRequest(request);
            List<TransactionHistoryItemDto> transactions = _transactionHistoryRepository.GetTransactionsByUserId(userId);
            List<TransactionHistoryItemDto> filteredTransactions = ApplyFiltersAndSort(transactions, normalizedRequest);
            TransactionExportResult exportResult = _transactionExportService.ExportStatement(filteredTransactions, normalizedRequest, request.Format);
            return File(exportResult.Content, exportResult.ContentType, exportResult.FileName);
        }

        [HttpGet("{transactionId:int}/receipt")]
        public IActionResult ExportReceipt(int transactionId)
        {
            int userId = GetAuthenticatedUserId();
            TransactionHistoryItemDto? transaction = _transactionHistoryRepository.GetTransactionById(userId, transactionId);
            TransactionExportResult exportResult = transaction == null ? new TransactionExportResult() : _transactionExportService.ExportReceipt(transaction);
            if (exportResult.Content.Length == 0)
            {
                return NotFound();
            }

            return File(exportResult.Content, exportResult.ContentType, exportResult.FileName);
        }

        private static List<TransactionHistoryItemDto> ApplyFiltersAndSort(IEnumerable<TransactionHistoryItemDto> transactions, TransactionHistoryRequest request)
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
