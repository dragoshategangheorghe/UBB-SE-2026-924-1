namespace BankApp.Models.DTOs.Statistics
{
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
}
