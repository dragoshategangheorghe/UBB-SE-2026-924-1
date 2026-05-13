using System;
using System.Collections.Generic;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Features.Loans;

namespace BankApp.Client.Utilities
{
    public static class AmortizationCalculator
    {
        private const decimal ZeroDecimal = 0m;
        private const int FirstInstallmentNumber = 1;
        private const decimal OneDecimal = 1m;
        private const decimal MonthsPerYear = 12m;
        private const decimal PercentageScale = 100m;
        private const int CurrencyPrecisionDigits = 2;

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

        public static decimal ComputeRepaymentProgress(decimal principal, decimal outstandingBalance)
        {
            if (principal == ZeroDecimal)
            {
                return ZeroDecimal;
            }

            return (principal - outstandingBalance) / principal * PercentageScale;
        }

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

