using BankApp.Models.DTOs.Transactions;

namespace BankApp.Web.Models.Transactions
{
    public class TransactionHistoryPageViewModel
    {
        public List<TransactionHistoryItemDto> Transactions { get; set; } = new();

        public string SearchTerm { get; set; } = string.Empty;

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal? MinimumAmount { get; set; }

        public decimal? MaximumAmount { get; set; }

        public int? SelectedAccountId { get; set; }

        public int? SelectedCardId { get; set; }

        public string SelectedTransactionType { get; set; } = string.Empty;

        public string SelectedStatus { get; set; } = string.Empty;

        public string SelectedDirection { get; set; } = string.Empty;

        public string SelectedSortField { get; set; } = TransactionSortFields.Date;

        public string SelectedSortDirection { get; set; } = SortDirections.Desc;

        public int? SelectedTransactionId { get; set; }

        public TransactionHistoryItemDto? SelectedTransaction =>
            SelectedTransactionId.HasValue
                ? Transactions.FirstOrDefault(transaction => transaction.Id == SelectedTransactionId.Value) ?? Transactions.FirstOrDefault()
                : Transactions.FirstOrDefault();

        public List<TransactionFilterItemViewModel> AccountOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = string.Empty, Label = "All Accounts" }
        };

        public List<TransactionFilterItemViewModel> CardOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = string.Empty, Label = "All Cards" }
        };

        public List<TransactionFilterItemViewModel> TransactionTypeOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = string.Empty, Label = "All Types" }
        };

        public List<TransactionFilterItemViewModel> StatusOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = string.Empty, Label = "All Statuses" }
        };

        public List<TransactionFilterItemViewModel> DirectionOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = string.Empty, Label = "All Directions" }
        };

        public List<TransactionSortItemViewModel> SortOptions { get; set; } = new()
        {
            new TransactionSortItemViewModel
            {
                Value = TransactionSortFields.Date,
                Label = "Date"
            },
            new TransactionSortItemViewModel
            {
                Value = TransactionSortFields.Amount,
                Label = "Amount"
            }
        };

        public List<TransactionSortItemViewModel> SortDirectionOptions { get; set; } = new()
        {
            // These are flipped for some reason
            new TransactionSortItemViewModel
            {
                Value = SortDirections.Asc,
                Label = "Descending"
            },
            new TransactionSortItemViewModel
            {
                Value = SortDirections.Desc,
                Label = "Ascending"
            }
        };

        public string LastExportPath { get; set; } = string.Empty;

        public string StatusMessage { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }

        public TransactionHistoryRequest ToHistoryRequest()
        {
            return new TransactionHistoryRequest
            {
                SearchTerm = SearchTerm,
                FromDate = FromDate,
                ToDate = ToDate,
                TransactionType = SelectedTransactionType,
                MinimumAmount = MinimumAmount,
                MaximumAmount = MaximumAmount,
                AccountId = SelectedAccountId,
                CardId = SelectedCardId,
                Status = SelectedStatus,
                Direction = SelectedDirection,
                SortField = SelectedSortField,
                SortDirection = SelectedSortDirection
            };
        }

        public TransactionExportRequest ToExportRequest(string format)
        {
            return new TransactionExportRequest
            {
                Format = format,
                SearchTerm = SearchTerm,
                FromDate = FromDate,
                ToDate = ToDate,
                TransactionType = SelectedTransactionType,
                MinimumAmount = MinimumAmount,
                MaximumAmount = MaximumAmount,
                AccountId = SelectedAccountId,
                CardId = SelectedCardId,
                Status = SelectedStatus,
                Direction = SelectedDirection,
                SortField = SelectedSortField,
                SortDirection = SelectedSortDirection
            };
        }

        public void ApplyFilters(TransactionHistoryRequest filters)
        {
            SearchTerm = filters.SearchTerm ?? string.Empty;
            FromDate = filters.FromDate;
            ToDate = filters.ToDate;
            SelectedTransactionType = filters.TransactionType ?? string.Empty;
            MinimumAmount = filters.MinimumAmount;
            MaximumAmount = filters.MaximumAmount;
            SelectedAccountId = filters.AccountId;
            SelectedCardId = filters.CardId;
            SelectedStatus = filters.Status ?? string.Empty;
            SelectedDirection = filters.Direction ?? string.Empty;
            SelectedSortField = filters.SortField;
            SelectedSortDirection = filters.SortDirection;
        }
    }

    public class TransactionFilterItemViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class TransactionSortItemViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
