using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;

namespace BankApp.Web.Models.Loans;

public class LoansPageViewModel
{
    public List<LoanCardViewModel> Loans { get; set; } = [];

    public LoanApplicationFormModel Application { get; set; } = new();

    public LoanPaymentFormModel Payment { get; set; } = new();

    public LoanStatus? SelectedStatusFilter { get; set; }

    public LoanType? SelectedTypeFilter { get; set; }

    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public bool HasLoans => Loans.Count > 0;
}

public class LoanCardViewModel
{
    public required Loan Loan { get; set; }

    public double RepaymentProgress { get; set; }

    public int PaidInstallments => Loan.TermInMonths - Loan.RemainingMonths;
}

public class LoanApplicationFormModel
{
    [Display(Name = "Loan type")]
    public LoanType LoanType { get; set; } = LoanType.Personal;

    [Display(Name = "Desired amount")]
    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal DesiredAmount { get; set; }

    [Display(Name = "Preferred term (months)")]
    [Range(1, 480)]
    public int PreferredTermMonths { get; set; }

    [Required]
    [Display(Name = "Purpose")]
    public string Purpose { get; set; } = string.Empty;

    public LoanEstimate? Estimate { get; set; }
}

public class LoanPaymentFormModel
{
    public int LoanId { get; set; }

    public bool UseCustomAmount { get; set; }

    public string CustomAmount { get; set; } = string.Empty;
}

public class LoanPaymentPreviewViewModel
{
    public decimal BalanceAfterPayment { get; set; }

    public int RemainingMonthsAfterPayment { get; set; }

    public string? ErrorMessage { get; set; }
}

public class LoanSchedulePageViewModel
{
    public required LoanCardViewModel Loan { get; set; }

    public List<AmortizationRow> Rows { get; set; } = [];
}
