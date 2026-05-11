using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.Enums;

namespace BankApp.Models.Features.Savings
{
    /// <summary>
    /// Represents an automatic recurring transfer into a savings account.
    /// </summary>
    public class AutoDeposit
    {
        /// <summary>
        /// Gets or sets the unique auto-deposit identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key from SavingsAccount
        /// </summary>
        [Column("savingsAccountId")]
        public int SavingsAccountId { get; set; }

        /// <summary>
        /// Gets or sets the linked savings account.
        /// </summary>
        [ForeignKey("SavingsAccountId")]
        public virtual SavingsAccount SavingsAccount { get; set; } = null!;

        /// <summary>
        /// Gets or sets the amount transferred each run.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets how often the transfer executes.
        /// </summary>
        public DepositFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the scheduled date of the next transfer.
        /// </summary>
        public DateTime NextRunDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether scheduling is enabled.
        /// </summary>
        public bool IsActive { get; set; }

        [Column("sourceAccountId")]
        public int? SourceAccountId { get; set; }

        [Column("dayOfMonth")]
        public int? DayOfMonth { get; set; }

        [Column("dayOfWeek")]
        public int? DayOfWeek { get; set; }

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
