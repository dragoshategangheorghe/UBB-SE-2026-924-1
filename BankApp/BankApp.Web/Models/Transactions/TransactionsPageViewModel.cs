using System;
using System.Collections.Generic;
using BankApp.Models.DTOs.Transactions;

namespace BankApp.Web.Models.Transactions
{
    public class TransactionHistoryPageViewModel
    {
        public List<TransactionHistoryItemDto> Transactions { get; set; } = new();

        // Filters
        public string SearchTerm { get; set; } = string.Empty;

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal? MinimumAmount { get; set; }

        public decimal? MaximumAmount { get; set; }

        public string SelectedTransactionType { get; set; } = string.Empty;

        public string SelectedStatus { get; set; } = string.Empty;

        public string SelectedDirection { get; set; } = string.Empty;

        public string SelectedSortField { get; set; } = TransactionSortOptions.Date;

        public string SelectedSortDirection { get; set; } = SortDirections.Desc;

        // Dropdown Options
        public List<TransactionFilterItemViewModel> TransactionTypeOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = "", Label = "All Types" },
            new TransactionFilterItemViewModel { Value = "Transfer", Label = "Transfer" },
            new TransactionFilterItemViewModel { Value = "CardPayment", Label = "Card Payment" },
            new TransactionFilterItemViewModel { Value = "Withdrawal", Label = "Withdrawal" },
            new TransactionFilterItemViewModel { Value = "Deposit", Label = "Deposit" }
        };

        public List<TransactionFilterItemViewModel> StatusOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = "", Label = "All Statuses" },
            new TransactionFilterItemViewModel { Value = "Completed", Label = "Completed" },
            new TransactionFilterItemViewModel { Value = "Pending", Label = "Pending" },
            new TransactionFilterItemViewModel { Value = "Failed", Label = "Failed" }
        };

        public List<TransactionFilterItemViewModel> DirectionOptions { get; set; } = new()
        {
            new TransactionFilterItemViewModel { Value = "", Label = "All Directions" },
            new TransactionFilterItemViewModel { Value = "Incoming", Label = "Incoming" },
            new TransactionFilterItemViewModel { Value = "Outgoing", Label = "Outgoing" }
        };

        public List<TransactionSortItemViewModel> SortOptions { get; set; } = new()
        {
            new TransactionSortItemViewModel
            {
                Value = TransactionSortOptions.Date,
                Label = "Date"
            },
            new TransactionSortItemViewModel
            {
                Value = TransactionSortOptions.Amount,
                Label = "Amount"
            },
            new TransactionSortItemViewModel
            {
                Value = TransactionSortOptions.Status,
                Label = "Status"
            },
            new TransactionSortItemViewModel
            {
                Value = TransactionSortOptions.Type,
                Label = "Type"
            }
        };

        public List<TransactionSortItemViewModel> SortDirectionOptions { get; set; } = new()
        {
            new TransactionSortItemViewModel
            {
                Value = SortDirections.Asc,
                Label = "Ascending"
            },
            new TransactionSortItemViewModel
            {
                Value = SortDirections.Desc,
                Label = "Descending"
            }
        };

        // Export
        public string LastExportPath { get; set; } = string.Empty;

        // Status
        public string StatusMessage { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }
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

    public static class TransactionSortOptions
    {
        public const string Date = "Date";

        public const string Amount = "Amount";

        public const string Status = "Status";

        public const string Type = "Type";
    }

    public static class SortDirectionOptions
    {
        public const string Ascending = "Ascending";

        public const string Descending = "Descending";
    }
}