using System.Collections.Generic;
using BankApp.Models.DTOs.Cards;

namespace BankApp.Web.Models.Cards
{
    public class CardManagementPageViewModel
    {
        public List<CardSummaryDto> Cards { get; set; } = new();
        public CardSummaryDto? SelectedCard { get; set; }
        public string SelectedSortOption { get; set; } = CardSortOptions.Custom;

        public List<CardSortItemViewModel> SortOptions { get; set; } = new()
        {
            new CardSortItemViewModel { Value = CardSortOptions.Custom, Label = "Custom" },
            new CardSortItemViewModel { Value = CardSortOptions.CardholderName, Label = "Cardholder Name" },
            new CardSortItemViewModel { Value = CardSortOptions.ExpiryDate, Label = "Expiry Date" },
            new CardSortItemViewModel { Value = CardSortOptions.Status, Label = "Status" }
        };

        public string StatusMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }

    public class CardSortItemViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}

