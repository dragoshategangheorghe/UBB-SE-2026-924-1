namespace BankApp.Models.DTOs.Statistics
{
    public class SpendingByCategoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal TotalSpending { get; set; }
        public List<CategorySpendingPointDto> Categories { get; set; } = new ();
    }
}
