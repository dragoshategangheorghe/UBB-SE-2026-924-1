namespace BankApp.Models.DTOs.Statistics
{
    public class IncomeVsExpensesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Net { get; set; }
    }
}
