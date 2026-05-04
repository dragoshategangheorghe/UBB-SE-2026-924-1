using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.Enums;

namespace BankApp.Models.Features.Savings
{
    public class SavingsTransaction
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for a savings account involved in the transaction.
        /// </summary>
        public int SavingsAccountId { get; set; }

        /// <summary>
        ///     Gets or sets the amount of money involved in the transaction.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the type of transaction, which can be either a deposit or a withdrawal.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        ///     Gets or sets the source of the transaction, which can be used to provide additional context about where the money
        ///     came from or where it is going.
        /// </summary>
        public string? Source { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the date and time when the transaction was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the account associated with this transaction.
        ///     This is the account that the transaction is being made on, and it is used to link the transaction to the correct
        ///     account in the database.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        ///     Gets or sets the balance of the savings account after the transaction has been processed.
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        ///     Gets or sets an optional description for the transaction, which can be used to provide additional details about the
        ///     transaction for the user's reference.
        /// </summary>
        public string? Description { get; set; }
    }
}
