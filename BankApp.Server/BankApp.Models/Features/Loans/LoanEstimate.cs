// <copyright file="LoanEstimate.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

using System;

/// <summary>
/// Represents a preliminary quote computed for a loan request.
/// </summary>
namespace BankApp.Models.Features.Loans
{
    public class LoanEstimate : IEquatable<LoanEstimate>
    {
        /// <summary>
        /// Gets or sets the indicative annual interest rate.
        /// </summary>
        public decimal IndicativeRate { get; set; }

        /// <summary>
        /// Gets or sets the projected monthly installment.
        /// </summary>
        public decimal MonthlyInstallment { get; set; }

        /// <summary>
        /// Gets or sets the estimated total amount repayable over the term.
        /// </summary>
        public decimal TotalRepayable { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            // Folosim pattern matching pentru a verifica tipul
            return Equals(obj as LoanEstimate);
        }

        /// <summary>
        /// Determines whether the specified LoanEstimate is equal to the current LoanEstimate.
        /// </summary>
        public bool Equals(LoanEstimate other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return IndicativeRate == other.IndicativeRate &&
                   MonthlyInstallment == other.MonthlyInstallment &&
                   TotalRepayable == other.TotalRepayable;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(IndicativeRate, MonthlyInstallment, TotalRepayable);
        }

        public static bool operator ==(LoanEstimate left, LoanEstimate right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        public static bool operator !=(LoanEstimate left, LoanEstimate right)
        {
            return !(left == right);
        }
    }
}