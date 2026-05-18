using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Transactions;
using BankApp.Web.Infrastructure;
using BankApp.Web.Models.Transactions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BankApp.Web.Controllers
{
    public class TransactionsController : WebControllerBase
    {
        private readonly ITransactionHistoryService _transactionService;

        public TransactionsController(ITransactionHistoryService transactionService, IWebSessionContext sessionContext)
            : base(sessionContext)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TransactionHistoryPageViewModel model, [FromQuery] int? selectedTransactionId = null)
        {
            try
            {
                var request = model.ToHistoryRequest();
                TransactionHistoryResponse? response = await _transactionService.GetHistoryAsync(request);

                if (response?.Success == true)
                {
                    model.Transactions = response.Transactions;
                    model.ApplyFilters(response.AppliedFilters);
                    model.SelectedTransactionId = ResolveSelectedTransactionId(model.Transactions, selectedTransactionId ?? model.SelectedTransactionId);
                }
                else
                {
                    model.Transactions = new List<TransactionHistoryItemDto>();
                    model.StatusMessage = response?.Message ?? "Failed to load transactions.";
                    model.IsSuccess = false;
                }

                await PopulateFilterOptionsAsync(model);
                string? statusMessage = TempData["StatusMessage"] as string;
                string? errorMessage = TempData["ErrorMessage"] as string;
                model.StatusMessage = statusMessage ?? errorMessage ?? model.StatusMessage;
                model.LastExportPath = TempData["LastExportPath"] as string ?? model.LastExportPath;
                model.IsSuccess = string.IsNullOrWhiteSpace(model.StatusMessage) || statusMessage != null;

                return View(model);
            }
            catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
            {
                return result;
            }
            catch (Exception exception)
            {
                model.Transactions = new List<TransactionHistoryItemDto>();
                model.SelectedTransactionId = selectedTransactionId ?? model.SelectedTransactionId;
                model.StatusMessage = exception.Message;
                model.IsSuccess = false;
                await PopulateFilterOptionsAsync(model);
                return View(model);
            }
        }

        [HttpGet]
        public Task<IActionResult> ExportCsv([FromQuery] TransactionHistoryPageViewModel model) =>
            ExportAsync(model, TransactionExportFormats.Csv);

        [HttpGet]
        public Task<IActionResult> ExportPdf([FromQuery] TransactionHistoryPageViewModel model) =>
            ExportAsync(model, TransactionExportFormats.Pdf);

        [HttpGet]
        public Task<IActionResult> ExportXlsx([FromQuery] TransactionHistoryPageViewModel model) =>
            ExportAsync(model, TransactionExportFormats.Xlsx);

        [HttpGet]
        public async Task<IActionResult> ExportReceipt(int transactionId)
        {
            try
            {
                ExportedFileResult? result = await _transactionService.ExportReceiptAsync(transactionId);
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Could not export the selected receipt.";
                }
                else
                {
                    TempData["StatusMessage"] = $"Receipt exported to {result.FilePath}.";
                    TempData["LastExportPath"] = result.FilePath;
                }
            }
            catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var redirect))
            {
                return redirect;
            }
            catch (Exception exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Index), new { selectedTransactionId = transactionId });
        }

        private async Task<IActionResult> ExportAsync(TransactionHistoryPageViewModel model, string format)
        {
            try
            {
                ExportedFileResult? result = await _transactionService.ExportTransactionsAsync(model.ToExportRequest(format));
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Could not export transactions.";
                }
                else
                {
                    TempData["StatusMessage"] = $"Transactions exported to {result.FilePath}.";
                    TempData["LastExportPath"] = result.FilePath;
                }
            }
            catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var redirect))
            {
                return redirect;
            }
            catch (Exception exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Index), ToRouteValues(model));
        }

        private async Task PopulateFilterOptionsAsync(TransactionHistoryPageViewModel model)
        {
            TransactionFilterMetadataResponse? metadata;
            try
            {
                metadata = await _transactionService.GetFilterMetadataAsync();
            }
            catch (HttpRequestException exception) when (exception.StatusCode != HttpStatusCode.Unauthorized)
            {
                return;
            }

            if (metadata?.Success != true)
            {
                return;
            }

            model.AccountOptions = BuildOptions(
                "All Accounts",
                metadata.Accounts.Select(account => new TransactionFilterItemViewModel
                {
                    Value = account.Id.ToString(),
                    Label = string.IsNullOrWhiteSpace(account.Iban) ? account.Name : $"{account.Name} ({account.Iban})"
                }));

            model.CardOptions = BuildOptions(
                "All Cards",
                metadata.Cards.Select(card => new TransactionFilterItemViewModel
                {
                    Value = card.Id.ToString(),
                    Label = card.Label
                }));

            model.TransactionTypeOptions = BuildOptions("All Types", metadata.AvailableTransactionTypes);
            model.StatusOptions = BuildOptions("All Statuses", metadata.AvailableStatuses);
            model.DirectionOptions = BuildOptions("All Directions", metadata.AvailableDirections);
        }

        private static List<TransactionFilterItemViewModel> BuildOptions(string allLabel, IEnumerable<string> values)
        {
            return BuildOptions(
                allLabel,
                values.Select(value => new TransactionFilterItemViewModel { Value = value, Label = value }));
        }

        private static List<TransactionFilterItemViewModel> BuildOptions(string allLabel, IEnumerable<TransactionFilterItemViewModel> values)
        {
            var options = new List<TransactionFilterItemViewModel>
            {
                new TransactionFilterItemViewModel { Value = string.Empty, Label = allLabel }
            };

            options.AddRange(values.Where(option => !string.IsNullOrWhiteSpace(option.Value)));
            return options;
        }

        private static object ToRouteValues(TransactionHistoryPageViewModel model)
        {
            return new
            {
                model.SearchTerm,
                model.FromDate,
                model.ToDate,
                model.MinimumAmount,
                model.MaximumAmount,
                model.SelectedAccountId,
                model.SelectedCardId,
                model.SelectedTransactionType,
                model.SelectedStatus,
                model.SelectedDirection,
                model.SelectedSortField,
                model.SelectedSortDirection,
                model.SelectedTransactionId
            };
        }

        private static int? ResolveSelectedTransactionId(List<TransactionHistoryItemDto> transactions, int? requestedTransactionId)
        {
            if (requestedTransactionId.HasValue && transactions.Any(transaction => transaction.Id == requestedTransactionId.Value))
            {
                return requestedTransactionId.Value;
            }

            return transactions.FirstOrDefault()?.Id;
        }
    }
}
