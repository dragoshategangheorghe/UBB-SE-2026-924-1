using System;
using BankApp.Models.Entities;
namespace BankApp.Models.DTOs.Dashboard
{
    public class DashboardResponse
    {
        public User CurrentUser { get; set; }
        public List<Card> Cards { get; set; } = new ();
        public List<Transaction> RecentTransactions { get; set; } = new ();
        public int UnreadNotificationCount { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not DashboardResponse)
            {
                return false;
            }

            var otherResponse = (DashboardResponse)obj;
            return CurrentUser.Equals(otherResponse.CurrentUser)
                && Cards.SequenceEqual(otherResponse.Cards)
                && RecentTransactions.SequenceEqual(otherResponse.RecentTransactions)
                && UnreadNotificationCount == otherResponse.UnreadNotificationCount;
        }
    }
}