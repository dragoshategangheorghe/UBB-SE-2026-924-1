namespace BankApp.Models.DTOs.Cards
{
    public class GetCardsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SortOption { get; set; } = CardSortOptions.Custom;
        public List<CardSummaryDto> Cards { get; set; } = new ();
    }
}
