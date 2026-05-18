using System;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;

namespace BankApp.Models.DTOs.Loans
{
    /// <summary>
    /// Minimal request contract for creating an approved loan record.
    /// </summary>
    public class LoanCreateDto
    {
        public int UserId { get; set; }

        public LoanType LoanType { get; set; }

        public decimal Principal { get; set; }

        public decimal OutstandingBalance { get; set; }

        public decimal InterestRate { get; set; }

        public decimal MonthlyInstallment { get; set; }

        public int RemainingMonths { get; set; }

        public LoanStatus LoanStatus { get; set; }

        public int TermInMonths { get; set; }

        public DateTime StartDate { get; set; }

        public static LoanCreateDto FromLoan(Loan loan)
        {
            return new LoanCreateDto
            {
                UserId = loan.UserId,
                LoanType = loan.LoanType,
                Principal = loan.Principal,
                OutstandingBalance = loan.OutstandingBalance,
                InterestRate = loan.InterestRate,
                MonthlyInstallment = loan.MonthlyInstallment,
                RemainingMonths = loan.RemainingMonths,
                LoanStatus = loan.LoanStatus,
                TermInMonths = loan.TermInMonths,
                StartDate = loan.StartDate,
            };
        }

        public Loan ToLoan()
        {
            return new Loan
            {
                UserId = this.UserId,
                LoanType = this.LoanType,
                Principal = this.Principal,
                OutstandingBalance = this.OutstandingBalance,
                InterestRate = this.InterestRate,
                MonthlyInstallment = this.MonthlyInstallment,
                RemainingMonths = this.RemainingMonths,
                LoanStatus = this.LoanStatus,
                TermInMonths = this.TermInMonths,
                StartDate = this.StartDate,
            };
        }
    }
}
