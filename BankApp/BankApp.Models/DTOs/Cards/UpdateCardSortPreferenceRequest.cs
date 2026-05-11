namespace BankApp.Models.DTOs.Cards
{
    public class UpdateCardSortPreferenceRequest
    {
        public string SortOption { get; set; } = CardSortOptions.Custom;
    }
}
