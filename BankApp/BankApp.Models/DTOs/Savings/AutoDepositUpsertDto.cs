using BankApp.Models.Enums;
using BankApp.Models.Features.Savings;

namespace BankApp.Models.DTOs.Savings
{
    /// <summary>
    /// Minimal request contract for creating or updating an auto-deposit schedule.
    /// </summary>
    public class AutoDepositUpsertDto
    {
        public int Id { get; set; }

        public int SavingsAccountId { get; set; }

        public decimal Amount { get; set; }

        public DepositFrequency Frequency { get; set; }

        public DateTime NextRunDate { get; set; }

        public bool IsActive { get; set; }

        public int? SourceAccountId { get; set; }

        public int? DayOfMonth { get; set; }

        public int? DayOfWeek { get; set; }

        public static AutoDepositUpsertDto FromAutoDeposit(AutoDeposit autoDeposit)
        {
            return new AutoDepositUpsertDto
            {
                Id = autoDeposit.Id,
                SavingsAccountId = autoDeposit.SavingsAccountId != 0
                    ? autoDeposit.SavingsAccountId
                    : autoDeposit.SavingsAccount?.IdentificationNumber ?? 0,
                Amount = autoDeposit.Amount,
                Frequency = autoDeposit.Frequency,
                NextRunDate = autoDeposit.NextRunDate,
                IsActive = autoDeposit.IsActive,
                SourceAccountId = autoDeposit.SourceAccountId ?? autoDeposit.SavingsAccount?.FundingAccount?.Id,
                DayOfMonth = autoDeposit.DayOfMonth,
                DayOfWeek = autoDeposit.DayOfWeek,
            };
        }

        public AutoDeposit ToAutoDeposit()
        {
            return new AutoDeposit
            {
                Id = this.Id,
                SavingsAccountId = this.SavingsAccountId,
                Amount = this.Amount,
                Frequency = this.Frequency,
                NextRunDate = this.NextRunDate,
                IsActive = this.IsActive,
                SourceAccountId = this.SourceAccountId,
                DayOfMonth = this.DayOfMonth,
                DayOfWeek = this.DayOfWeek,
            };
        }
    }
}
