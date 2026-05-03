using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.Features.Savings
{
    /// <summary>
    ///     This model is used to manage and display information about a user's savings accounts within the application.
    /// </summary>
    public class SavingsAccount
    {
        private const decimal MonthsInYear = 12m;
        private const decimal PercentageScale = 100m;
        private const decimal MinimumTargetAmountExclusive = 0m;
        private const double DefaultProgressPercent = 0d;

        /// <summary>
        ///     Gets or sets the unique identifier for the savings account.
        /// </summary>
        public int IdentificationNumber { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the user who owns the savings account.
        /// </summary>
        public int UserIdentificationNumber { get; set; }

        /// <summary>
        ///     Gets or sets the type of savings account, which can be "Standard", "GoalSavings", "FixedDeposit", or "HighYield".
        /// </summary>
        public string SavingsType { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the current balance of the savings account, which represents the total amount of money currently saved
        ///     in the account.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        ///     Gets or sets the total amount of interest that has been accrued on the savings account but has not yet been added
        ///     to the balance.
        /// </summary>
        public decimal AccruedInterest { get; set; }

        /// <summary>
        ///     Gets or sets the annual percentage yield (APY) for the savings account.
        /// </summary>
        public decimal AnnualPercentageYield { get; set; }

        /// <summary>
        ///     Gets or sets the maturity date for fixed deposit accounts, which indicates when the funds will be available without
        ///     penalty and when interest will be added to the balance.
        /// </summary>
        public DateTime? MaturityDate { get; set; }

        /// <summary>
        ///     Gets or sets the current status of the savings account, which can be "Active", "Inactive", "Closed", or "Matured".
        /// </summary>
        public string AccountStatus { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the date and time when the savings account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the date and time when the savings account was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the optional name of the savings account, which can be used by users to easily identify and
        ///     differentiate between multiple accounts.
        /// </summary>
        public string? AccountName { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the funding account associated with this savings account, if any.
        /// </summary>
        public int? FundingAccountIdentificationNumber { get; set; }

        /// <summary>
        ///     Gets or sets the target amount for goal savings accounts, which the user aims to achieve.
        /// </summary>
        public decimal? TargetAmount { get; set; }

        /// <summary>
        ///     Gets or sets the target date for goal savings accounts, when the user plans to reach their savings goal.
        /// </summary>
        public DateTime? TargetDate { get; set; }

        // Computed properties (not stored in DB)

        /// <summary>
        ///     Gets the projected monthly interest based on the current balance and APY.
        ///     This provides users with an estimate of how much interest they can expect to earn in a month if the balance remains
        ///     unchanged.
        /// </summary>
        public decimal MonthlyInterestProjection => this.Balance * this.AnnualPercentageYield / MonthsInYear;

        /// <summary>
        ///     Gets the percentage of the savings goal that has been achieved.
        /// </summary>
        public double ProgressPercent =>
            this.TargetAmount.HasValue && this.TargetAmount.Value > MinimumTargetAmountExclusive
                ? (double)(this.Balance / this.TargetAmount.Value * PercentageScale)
                : DefaultProgressPercent;

        /// <summary>
        ///     Gets the string representation of the balance of the account, prefixed with a dollar sign for a nice display.
        /// </summary>
        public string FormattedBalance => $"${this.Balance:N2}";

        /// <summary>
        ///     Gets a value indicating whether the type of the savings account is 'goal savings' or not.
        /// </summary>
        public bool IsGoalSavings => this.SavingsType == "GoalSavings";

        /// <summary>
        ///     Gets the status of the savings account.
        /// </summary>
        public string DisplayStatus =>
            this.SavingsType == "FixedDeposit" &&
            this.MaturityDate.HasValue &&
            this.MaturityDate.Value <= DateTime.UtcNow
                ? "Matured"
                : this.AccountStatus;
    }
}
