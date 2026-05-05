// <copyright file="TransactionType.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Enums;

/// <summary>
///     Represents the types of transactions that can occur on a savings account.
/// </summary>
public enum TransactionType
{
    /// <summary>
    ///     Represents the action of depositing funds into a savings account.
    /// </summary>
    Deposit,

    /// <summary>
    ///     Represents the action of withdrawing funds from a savings account.
    /// </summary>
    Withdrawal,

    /// <summary>
    ///     Represents the action of accruing interest on a savings account, which increases the account balance based on the
    ///     APY and the current balance.
    /// </summary>
    Interest,

    /// <summary>
    ///     Represents the action of transferring funds from one savings account to another, which involves both a withdrawal
    ///     from the source account and a deposit into the destination account.
    /// </summary>
    Transfer,

    /// <summary>
    ///     Represents the action of closing a savings account, which typically involves transferring the remaining balance to
    ///     another account and marking the savings account as closed.
    /// </summary>
    Closure,
}