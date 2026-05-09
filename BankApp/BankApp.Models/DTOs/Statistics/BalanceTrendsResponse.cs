namespace BankApp.Models.DTOs.Statistics
{
    public class BalanceTrendsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BalanceTrendPointDto> Points { get; set; } = new ();
    }
}
