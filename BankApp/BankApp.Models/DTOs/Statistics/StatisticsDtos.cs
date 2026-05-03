using BankApp.Models.Entities;

namespace BankApp.Models.DTOs.Statistics
{
    public class SpendingByCategoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal TotalSpending { get; set; }
        public List<CategorySpendingPointDto> Categories { get; set; } = new ();
    }

    public class IncomeVsExpensesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Net { get; set; }
    }

    public class BalanceTrendsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BalanceTrendPointDto> Points { get; set; } = new ();
    }

    public class TopRecipientsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TopCounterpartyDto> Recipients { get; set; } = new ();
    }

    public class CategorySpendingPointDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal ShareOfTotal { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not CategorySpendingPointDto)
            {
                return false;
            }
            var other = (CategorySpendingPointDto)obj;
            return CategoryName == other.CategoryName
                && Amount == other.Amount
                && ShareOfTotal == other.ShareOfTotal;
        }
    }

    public class BalanceTrendPointDto
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not BalanceTrendPointDto)
            {
                return false;
            }
            var other = (BalanceTrendPointDto)obj;
            return Date.Date == other.Date.Date
                && Balance == other.Balance;
        }
    }

    public class TopCounterpartyDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }
}
