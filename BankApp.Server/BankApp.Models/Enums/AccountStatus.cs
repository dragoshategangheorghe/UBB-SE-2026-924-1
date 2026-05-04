// <copyright file="AccountStatus.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Enums;

/// <summary>
///     Represents the status of a savings account. It tracks the current state of a savings account and determines what
///     actions are allowed based on the account's status.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    ///     Represents an active savings account that is currently open and can accept deposits, withdrawals, and accrue
    ///     interest.
    /// </summary>
    Active,

    /// <summary>
    ///     Represents a closed savings account that has been closed by the user or the bank.
    /// </summary>
    Closed,

    /// <summary>
    ///     Represents a matured savings account that has reached its maturity date and can no longer accrue interest.
    /// </summary>
    Matured,
}