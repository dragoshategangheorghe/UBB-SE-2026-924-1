using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;

namespace BankApp.Web.Models.Savings;

public class SavingsPageViewModel
{
    public List<SavingsAccount> Accounts { get; set; } = [];

    public SavingsAccount? SelectedAccount { get; set; }

    public List<FundingSourceOption> FundingSources { get; set; } = [];

    public List<SavingsAccount> CloseDestinationAccounts { get; set; } = [];

    public SavingsCreateAccountFormModel CreateAccount { get; set; } = new();

    public SavingsDepositFormModel Deposit { get; set; } = new();

    public SavingsWithdrawFormModel Withdraw { get; set; } = new();

    public SavingsAutoDepositFormModel AutoDeposit { get; set; } = new();

    public SavingsCloseAccountFormModel CloseAccount { get; set; } = new();

    public SavingsWithdrawPreviewViewModel WithdrawPreview { get; set; } = new();

    public string DepositPreview { get; set; } = string.Empty;

    public string TotalSavedAmount { get; set; } = "$0.00";

    public string NumberOfAccountsText { get; set; } = "across 0 accounts";

    public string BestInterestRate { get; set; } = "0.00%";

    public string ActiveManageTab { get; set; } = "overview";

    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public bool HasAccounts => Accounts.Count > 0;
}

public class SavingsCreateAccountFormModel
{
    [Required]
    [Display(Name = "Savings type")]
    public string SelectedSavingsType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Account nickname")]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Initial deposit")]
    public string InitialDeposit { get; set; } = string.Empty;

    [Display(Name = "Funding source")]
    public int? FundingSourceId { get; set; }

    [Display(Name = "Deposit frequency")]
    public string SelectedFrequency { get; set; } = string.Empty;

    [Display(Name = "Target amount")]
    public string TargetAmount { get; set; } = string.Empty;

    [Display(Name = "Target date")]
    [DataType(DataType.Date)]
    public DateTime? TargetDate { get; set; }

    [Display(Name = "Maturity date")]
    [DataType(DataType.Date)]
    public DateTime? MaturityDate { get; set; }
}

public class SavingsDepositFormModel
{
    public int AccountId { get; set; }

    [Required]
    [Display(Name = "Amount")]
    public string Amount { get; set; } = string.Empty;

    public int? FundingSourceId { get; set; }
}

public class SavingsWithdrawFormModel
{
    public int AccountId { get; set; }

    [Required]
    [Display(Name = "Amount")]
    public string Amount { get; set; } = string.Empty;

    public int? DestinationId { get; set; }
}

public class SavingsAutoDepositFormModel
{
    public int AccountId { get; set; }

    [Required]
    [Display(Name = "Amount")]
    public string Amount { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Frequency")]
    public string Frequency { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class SavingsCloseAccountFormModel
{
    public int AccountId { get; set; }

    [Display(Name = "Destination account")]
    public int DestinationAccountId { get; set; }

    [Display(Name = "I understand this action is permanent.")]
    public bool Confirmed { get; set; }
}

public class SavingsWithdrawPreviewViewModel
{
    public bool HasEarlyRisk { get; set; }

    public bool HasPenalty { get; set; }

    public string PenaltySummary { get; set; } = string.Empty;

    public string PenaltyBreakdown { get; set; } = string.Empty;

    public string NetAmountText { get; set; } = string.Empty;
}
