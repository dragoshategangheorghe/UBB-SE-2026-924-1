using System;
using BankApp.Models.Features.Loans;

namespace BankApp.Models.DTOs.Loans
{
    /// <summary>
    /// Minimal request contract for saving amortization schedule rows.
    /// </summary>
    public class AmortizationRowUpsertDto
    {
        public int Id { get; set; }

        public int LoanId { get; set; }

        public int InstallmentNumber { get; set; }

        public DateTime DueDate { get; set; }

        public decimal PrincipalPortion { get; set; }

        public decimal InterestPortion { get; set; }

        public decimal RemainingBalance { get; set; }

        public bool IsCurrent { get; set; }

        public static AmortizationRowUpsertDto FromAmortizationRow(AmortizationRow row)
        {
            return new AmortizationRowUpsertDto
            {
                Id = row.Id,
                LoanId = row.LoanId,
                InstallmentNumber = row.InstallmentNumber,
                DueDate = row.DueDate,
                PrincipalPortion = row.PrincipalPortion,
                InterestPortion = row.InterestPortion,
                RemainingBalance = row.RemainingBalance,
                IsCurrent = row.IsCurrent,
            };
        }

        public AmortizationRow ToAmortizationRow()
        {
            return new AmortizationRow
            {
                Id = this.Id,
                LoanId = this.LoanId,
                InstallmentNumber = this.InstallmentNumber,
                DueDate = this.DueDate,
                PrincipalPortion = this.PrincipalPortion,
                InterestPortion = this.InterestPortion,
                RemainingBalance = this.RemainingBalance,
                IsCurrent = this.IsCurrent,
            };
        }
    }
}
