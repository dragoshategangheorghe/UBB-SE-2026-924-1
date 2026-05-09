namespace BankApp.Models.DTOs.Statistics
{
    public class TopCounterpartyDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }
}
