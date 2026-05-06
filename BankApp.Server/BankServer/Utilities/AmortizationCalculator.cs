namespace BankApp.Server.Utilities
{
    using BankApp.Models.Features.Loans;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides utility methods for calculating loan amortization schedules and estimates.
    /// </summary>
    public static class AmortizationCalculator
    {
        private const decimal ZeroDecimal = 0m;
        private const int ZeroInteger = 0;
        private const int FirstInstallmentNumber = 1;
        private const decimal OneDecimal = 1m;
        private const decimal MonthsPerYear = 12m;
        private const decimal PercentageScale = 100m;
        private const int CurrencyPrecisionDigits = 2;

        /// <summary>
        /// Computes a loan estimate based on the requested amount, annual rate, and term.
        /// </summary>
        /// <param name="amount">The desired loan amount.</param>
        /// <param name="annualRate">The annual interest rate as a percentage.</param>
        /// <param name="termMonths">The term of the loan in months.</param>
        /// <returns>A <see cref="LoanEstimate"/> containing the indicative rate, monthly installment, and total repayable amount.</returns>
        public static LoanEstimate ComputeEstimate(decimal amount, decimal annualRate, int termMonths)
        {
            var monthlyRate = annualRate / MonthsPerYear / PercentageScale;
            decimal monthlyInstallment;

            if (monthlyRate == ZeroDecimal)
            {
                monthlyInstallment = amount / termMonths;
            }
            else
            {
                monthlyInstallment = amount * monthlyRate * (decimal)Math.Pow((double)(OneDecimal + monthlyRate), termMonths) /
                                     ((decimal)Math.Pow((double)(OneDecimal + monthlyRate), termMonths) - OneDecimal);
            }

            monthlyInstallment = Math.Round(monthlyInstallment, CurrencyPrecisionDigits);
            var totalRepayable = Math.Round(monthlyInstallment * termMonths, CurrencyPrecisionDigits);

            return new LoanEstimate
            {
                IndicativeRate = annualRate,
                MonthlyInstallment = monthlyInstallment,
                TotalRepayable = totalRepayable,
            };
        }

        /// <summary>
        /// Computes the repayment progress percentage based on the principal and outstanding balance.
        /// </summary>
        /// <param name="principal">The original principal amount of the loan.</param>
        /// <param name="outstandingBalance">The current outstanding balance of the loan.</param>
        /// <returns>A percentage representing the repayment progress.</returns>
        public static decimal ComputeRepaymentProgress(decimal principal, decimal outstandingBalance)
        {
            if (principal == ZeroDecimal)
            {
                return ZeroDecimal;
            }

            return (principal - outstandingBalance) / principal * PercentageScale;
        }

        /// <summary>
        /// Generates an amortization schedule for a given loan.
        /// </summary>
        /// <param name="loan">The loan details used to generate the schedule.</param>
        /// <returns>A list of <see cref="AmortizationRow"/> representing the amortization schedule.</returns>
        public static List<AmortizationRow> Generate(Loan loan)
        {
            var rows = new List<AmortizationRow>();

            var principal = loan.Principal;
            var annualRate = loan.InterestRate;
            var termInMonths = loan.TermInMonths;
            var startDate = loan.StartDate;

            var monthlyRate = annualRate / MonthsPerYear / PercentageScale;
            var remainingBalance = principal;
            decimal monthlyInstallment;

            if (monthlyRate == ZeroDecimal)
            {
                monthlyInstallment = remainingBalance / termInMonths;
            }
            else
            {
                monthlyInstallment = remainingBalance * monthlyRate *
                                     (decimal)Math.Pow((double)(OneDecimal + monthlyRate), termInMonths) /
                                     ((decimal)Math.Pow((double)(OneDecimal + monthlyRate), termInMonths) - OneDecimal);
            }

            monthlyInstallment = Math.Round(monthlyInstallment, CurrencyPrecisionDigits);

            var isCurrentMarked = false;

            for (var index = FirstInstallmentNumber; index <= termInMonths; index++)
            {
                var dueDate = startDate.AddMonths(index);
                var interestPortion = Math.Round(remainingBalance * monthlyRate, CurrencyPrecisionDigits);
                var principalPortion = monthlyInstallment - interestPortion;

                if (index == termInMonths)
                {
                    // Adjust final installment so remaining balance becomes exactly zero.
                    principalPortion = remainingBalance;
                    monthlyInstallment = principalPortion + interestPortion;
                }

                remainingBalance -= principalPortion;

                if (remainingBalance < ZeroDecimal || index == termInMonths)
                {
                    remainingBalance = ZeroDecimal;
                }

                var row = new AmortizationRow
                {
                    LoanId = loan.Id,
                    InstallmentNumber = index,
                    DueDate = dueDate,
                    PrincipalPortion = principalPortion,
                    InterestPortion = interestPortion,
                    RemainingBalance = remainingBalance,
                    IsCurrent = false,
                };

                if (!isCurrentMarked && dueDate.Date >= DateTime.Today)
                {
                    row.IsCurrent = true;
                    isCurrentMarked = true;
                }

                rows.Add(row);
            }

            return rows;
        }
    }
}