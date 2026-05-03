namespace BankApp.Server.Configuration
{
    public class TeamCOptions
    {
        public const string SectionName = "TeamC";

        public int CardRevealDurationSeconds { get; set; } = 30;
        public decimal MaximumSpendingLimit { get; set; } = 100000m;
        public int BalanceTrendDays { get; set; } = 30;
        public int TopRecipientsCount { get; set; } = 5;
    }
}
