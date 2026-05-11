namespace BankApp.Models.DTOs.Cards
{
    public class UpdateCardSettingsRequest
    {
        public decimal? SpendingLimit { get; set; }
        public bool? IsOnlinePaymentsEnabled { get; set; }
        public bool? IsContactlessPaymentsEnabled { get; set; }
    }
}
