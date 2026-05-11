namespace BankApp.Models.DTOs.Statistics
{
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
}
