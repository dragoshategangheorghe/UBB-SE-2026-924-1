namespace BankApp.Models.DTOs.Statistics
{
    public class TopRecipientsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TopCounterpartyDto> Recipients { get; set; } = new ();
    }
}
