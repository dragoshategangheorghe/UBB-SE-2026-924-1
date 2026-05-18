using System;
using System.Globalization;
using System.Linq;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Web.Infrastructure;
using BankApp.Web.Models.Savings;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers;

//[Authorize]
public class SavingsController : WebControllerBase
{
    private const string OverviewTab = "overview";
    private const string DepositTab = "deposit";
    private const string WithdrawTab = "withdraw";
    private const string AutoDepositTab = "auto";
    private const string CloseTab = "close";

    private readonly ISavingsService _savingsService;

    public SavingsController(ISavingsService savingsService, IWebSessionContext sessionContext)
        : base(sessionContext)
    {
        _savingsService = savingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? accountId = null, string manageTab = OverviewTab)
    {
        try
        {
            var model = await BuildPageModelAsync(accountId, manageTab);
            return View(model);
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            return View(new SavingsPageViewModel
            {
                ActiveManageTab = manageTab,
                ErrorMessage = exception.Message,
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount([Bind(Prefix = "CreateAccount")] SavingsCreateAccountFormModel createAccount)
    {
        try
        {
            var fundingSources = await _savingsService.GetFundingSourcesAsync(CurrentUserId);
            NormalizeCreateAccountForm(createAccount, fundingSources);
            ModelState.Clear();

            decimal? parsedTargetAmount = null;
            if (IsGoalSavings(createAccount.SelectedSavingsType) && !string.IsNullOrWhiteSpace(createAccount.TargetAmount))
            {
                try
                {
                    parsedTargetAmount = await _savingsService.ParsePositiveAmountAsync(createAccount.TargetAmount);
                }
                catch (InvalidOperationException)
                {
                    parsedTargetAmount = null;
                }
            }

            var validation = await _savingsService.ValidateCreateAccountAsync(new ValidateCreateAccountRequest
            {
                SelectedSavingsType = createAccount.SelectedSavingsType,
                AccountName = createAccount.AccountName,
                InitialDepositText = createAccount.InitialDeposit,
                HasFundingSource = createAccount.FundingSourceId.HasValue,
                SelectedFrequency = createAccount.SelectedFrequency,
                TargetAmount = parsedTargetAmount,
                TargetDate = createAccount.TargetDate.HasValue ? new DateTimeOffset(createAccount.TargetDate.Value) : null,
                IsGoalSavings = IsGoalSavings(createAccount.SelectedSavingsType),
            });

            AddCreateAccountErrors(validation);

            if (validation.Count > 0)
            {
                var invalidModel = await BuildPageModelAsync(null, OverviewTab, createAccount);
                invalidModel.ErrorMessage = "Please correct the highlighted fields.";
                return View("Index", invalidModel);
            }

            decimal initialDeposit = await _savingsService.ParsePositiveAmountAsync(createAccount.InitialDeposit);
            DepositFrequency? depositFrequency = string.Equals(createAccount.SelectedFrequency, "OneTime", StringComparison.OrdinalIgnoreCase) ||
                                                 string.IsNullOrWhiteSpace(createAccount.SelectedFrequency)
                ? null
                : await _savingsService.ParseDepositFrequencyAsync(createAccount.SelectedFrequency);

            await _savingsService.CreateAccountAsync(new CreateSavingsAccountDto
            {
                UserIdentificationNumber = CurrentUserId,
                SavingsType = createAccount.SelectedSavingsType,
                AccountName = createAccount.AccountName.Trim(),
                InitialDeposit = initialDeposit,
                FundingAccountId = createAccount.FundingSourceId!.Value,
                TargetAmount = IsGoalSavings(createAccount.SelectedSavingsType) ? parsedTargetAmount : null,
                TargetDate = IsGoalSavings(createAccount.SelectedSavingsType) ? createAccount.TargetDate : null,
                MaturityDate = IsFixedDeposit(createAccount.SelectedSavingsType) ? createAccount.MaturityDate : null,
                DepositFrequency = depositFrequency,
            });

            TempData["StatusMessage"] = "Savings account opened successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            var invalidModel = await BuildPageModelAsync(null, OverviewTab, createAccount);
            invalidModel.ErrorMessage = exception.Message;
            return View("Index", invalidModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit([Bind(Prefix = "Deposit")] SavingsDepositFormModel deposit)
    {
        try
        {
            decimal amount = await _savingsService.ParsePositiveAmountAsync(deposit.Amount);
            var fundingSources = await _savingsService.GetFundingSourcesAsync(CurrentUserId);
            var selectedFundingSource = fundingSources.FirstOrDefault(source => source.Id == deposit.FundingSourceId);
            if (selectedFundingSource == null)
            {
                TempData["ErrorMessage"] = "Select a funding source before submitting the deposit.";
                return RedirectToAction(nameof(Index), new { accountId = deposit.AccountId, manageTab = DepositTab });
            }

            var response = await _savingsService.DepositAsync(
                deposit.AccountId,
                amount,
                selectedFundingSource.DisplayName,
                CurrentUserId);

            TempData["StatusMessage"] = $"Deposit successful. New balance: {response.NewBalance:C2}.";
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = deposit.AccountId, manageTab = DepositTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw([Bind(Prefix = "Withdraw")] SavingsWithdrawFormModel withdraw)
    {
        try
        {
            decimal amount = await _savingsService.ParsePositiveAmountAsync(withdraw.Amount);
            var fundingSources = await _savingsService.GetFundingSourcesAsync(CurrentUserId);
            var destination = fundingSources.FirstOrDefault(source => source.Id == withdraw.DestinationId);
            var validation = await _savingsService.ValidateWithdrawRequestAsync(amount, destination);
            if (!validation.IsValid)
            {
                TempData["ErrorMessage"] = validation.ErrorMessage;
                return RedirectToAction(nameof(Index), new { accountId = withdraw.AccountId, manageTab = WithdrawTab });
            }

            var response = await _savingsService.WithdrawAsync(
                withdraw.AccountId,
                amount,
                destination!.DisplayName,
                CurrentUserId);

            var resultMessage = await _savingsService.BuildWithdrawResultMessageAsync(response);
            TempData[response.Success ? "StatusMessage" : "ErrorMessage"] = resultMessage;
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = withdraw.AccountId, manageTab = WithdrawTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAutoDeposit([Bind(Prefix = "AutoDeposit")] SavingsAutoDepositFormModel autoDeposit)
    {
        try
        {
            decimal amount = await _savingsService.ParsePositiveAmountAsync(autoDeposit.Amount);
            var frequency = await _savingsService.ParseDepositFrequencyAsync(autoDeposit.Frequency);
            var accounts = await _savingsService.GetAccountsAsync(CurrentUserId, true);
            var selectedAccount = accounts.FirstOrDefault(account => account.IdentificationNumber == autoDeposit.AccountId);
            if (selectedAccount == null)
            {
                TempData["ErrorMessage"] = "The selected account could not be found.";
                return RedirectToAction(nameof(Index), new { accountId = autoDeposit.AccountId, manageTab = AutoDepositTab });
            }

            var existingAutoDeposit = await _savingsService.GetAutoDepositAsync(autoDeposit.AccountId);
            await _savingsService.SaveAutoDepositAsync(new AutoDeposit
            {
                Id = existingAutoDeposit?.Id ?? 0,
                SavingsAccountId = autoDeposit.AccountId,
                SavingsAccount = selectedAccount,
                Amount = amount,
                Frequency = frequency,
                NextRunDate = autoDeposit.StartDate ?? DateTime.Today.AddDays(1),
                IsActive = autoDeposit.IsActive,
                SourceAccountId = selectedAccount.FundingAccount?.Id,
            });

            TempData["StatusMessage"] = "Auto deposit saved successfully.";
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = autoDeposit.AccountId, manageTab = AutoDepositTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseAccount([Bind(Prefix = "CloseAccount")] SavingsCloseAccountFormModel closeAccount)
    {
        try
        {
            var validation = await _savingsService.ValidateCloseConfirmationAsync(
                closeAccount.Confirmed,
                closeAccount.DestinationAccountId);

            if (!validation.IsValid)
            {
                TempData["ErrorMessage"] = validation.ErrorMessage;
                return RedirectToAction(nameof(Index), new { accountId = closeAccount.AccountId, manageTab = CloseTab });
            }

            var response = await _savingsService.CloseAccountAsync(
                closeAccount.AccountId,
                closeAccount.DestinationAccountId,
                CurrentUserId);

            TempData[response.Success ? "StatusMessage" : "ErrorMessage"] =
                response.Success
                    ? $"Account closed successfully. Transferred {response.TransferredAmount:C2}."
                    : response.Message;
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { accountId = closeAccount.AccountId, manageTab = CloseTab });
    }

    [HttpGet]
    public async Task<IActionResult> DepositPreview(int accountId, string amountText)
    {
        try
        {
            var accounts = await _savingsService.GetAccountsAsync(CurrentUserId, true);
            var selectedAccount = accounts.FirstOrDefault(account => account.IdentificationNumber == accountId);
            if (selectedAccount == null)
            {
                return Json(new { preview = string.Empty });
            }

            var preview = await _savingsService.GetDepositPreviewAsync(amountText, selectedAccount);
            return Json(new { preview });
        }
        catch (HttpRequestException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return Json(new { preview = string.Empty });
        }
    }

    [HttpGet]
    public async Task<IActionResult> WithdrawPreview(int accountId, string amountText)
    {
        try
        {
            var accounts = await _savingsService.GetAccountsAsync(CurrentUserId, true);
            var selectedAccount = accounts.FirstOrDefault(account => account.IdentificationNumber == accountId);
            if (selectedAccount == null)
            {
                return Json(new SavingsWithdrawPreviewViewModel());
            }

            var preview = await BuildWithdrawPreviewAsync(selectedAccount, amountText);
            return Json(preview);
        }
        catch (HttpRequestException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return Json(new SavingsWithdrawPreviewViewModel());
        }
    }

    private async Task<SavingsPageViewModel> BuildPageModelAsync(
        int? accountId,
        string manageTab,
        SavingsCreateAccountFormModel? createAccount = null)
    {
        var accounts = await _savingsService.GetAccountsAsync(CurrentUserId);
        var fundingSources = await _savingsService.GetFundingSourcesAsync(CurrentUserId);
        var selectedAccount = accounts.FirstOrDefault(account => account.IdentificationNumber == (accountId ?? accounts.FirstOrDefault()?.IdentificationNumber));

        var model = new SavingsPageViewModel
        {
            Accounts = accounts,
            FundingSources = fundingSources,
            SelectedAccount = selectedAccount,
            ActiveManageTab = manageTab,
            StatusMessage = TempData["StatusMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string,
            CreateAccount = createAccount ?? new SavingsCreateAccountFormModel
            {
                FundingSourceId = fundingSources.FirstOrDefault()?.Id,
                SelectedFrequency = "OneTime",
            },
            TotalSavedAmount = await _savingsService.GetTotalSavedAmountAsync(accounts),
            NumberOfAccountsText = await _savingsService.GetNumberOfAccountsTextAsync(accounts.Count),
            BestInterestRate = await _savingsService.GetBestInterestRateAsync(accounts),
        };

        if (selectedAccount == null)
        {
            return model;
        }

        model.Deposit = new SavingsDepositFormModel
        {
            AccountId = selectedAccount.IdentificationNumber,
            FundingSourceId = fundingSources.FirstOrDefault()?.Id,
        };

        model.Withdraw = new SavingsWithdrawFormModel
        {
            AccountId = selectedAccount.IdentificationNumber,
            DestinationId = fundingSources.FirstOrDefault()?.Id,
        };

        model.CloseDestinationAccounts = await _savingsService.GetValidTransferDestinationsAsync(
            selectedAccount.IdentificationNumber,
            CurrentUserId);

        model.CloseAccount = new SavingsCloseAccountFormModel
        {
            AccountId = selectedAccount.IdentificationNumber,
            DestinationAccountId = model.CloseDestinationAccounts.FirstOrDefault()?.IdentificationNumber ?? 0,
        };

        var existingAutoDeposit = await _savingsService.GetAutoDepositAsync(selectedAccount.IdentificationNumber);
        model.AutoDeposit = new SavingsAutoDepositFormModel
        {
            AccountId = selectedAccount.IdentificationNumber,
            Amount = existingAutoDeposit?.Amount.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
            Frequency = existingAutoDeposit?.Frequency.ToString() ?? "Monthly",
            StartDate = existingAutoDeposit?.NextRunDate ?? DateTime.Today.AddDays(1),
            IsActive = existingAutoDeposit?.IsActive ?? true,
        };

        model.WithdrawPreview = await BuildWithdrawPreviewAsync(selectedAccount, model.Withdraw.Amount);
        return model;
    }

    private async Task<SavingsWithdrawPreviewViewModel> BuildWithdrawPreviewAsync(SavingsAccount selectedAccount, string amountText)
    {
        var preview = new SavingsWithdrawPreviewViewModel
        {
            HasEarlyRisk = await _savingsService.HasRiskEarlyWithdrawal(selectedAccount),
        };

        if (!preview.HasEarlyRisk)
        {
            return preview;
        }

        preview.PenaltySummary = selectedAccount.MaturityDate.HasValue
            ? $"Early withdrawal penalty applies until {selectedAccount.MaturityDate.Value:dd MMM yyyy}."
            : "Early withdrawal penalty applies to this account.";

        try
        {
            decimal amount = await _savingsService.ParsePositiveAmountAsync(amountText);
            decimal penalty = await _savingsService.ComputeWithdrawalPenalty(amount);
            decimal netAmount = await _savingsService.GetWithdrawNetAmountAsync(amount, penalty);
            preview.HasPenalty = penalty > 0m;
            preview.PenaltyBreakdown = $"Penalty: {penalty:C2}";
            preview.NetAmountText = $"Net amount received: {netAmount:C2}";
        }
        catch (InvalidOperationException)
        {
            // Ignore incomplete input and keep the contextual warning only.
        }

        return preview;
    }

    private void AddCreateAccountErrors(System.Collections.Generic.Dictionary<string, string> errors)
    {
        foreach (var (key, value) in errors)
        {
            string modelStateKey = key switch
            {
                "SavingsType" => "CreateAccount.SelectedSavingsType",
                "AccountName" => "CreateAccount.AccountName",
                "InitialDeposit" => "CreateAccount.InitialDeposit",
                "FundingSource" => "CreateAccount.FundingSourceId",
                "Frequency" => "CreateAccount.SelectedFrequency",
                "TargetAmount" => "CreateAccount.TargetAmount",
                "TargetDate" => "CreateAccount.TargetDate",
                _ => string.Empty,
            };

            if (!string.IsNullOrWhiteSpace(modelStateKey))
            {
                ModelState.AddModelError(modelStateKey, value);
            }
        }
    }

    private static void NormalizeCreateAccountForm(
        SavingsCreateAccountFormModel createAccount,
        IReadOnlyList<FundingSourceOption> fundingSources)
    {
        createAccount.SelectedSavingsType = createAccount.SelectedSavingsType?.Trim() ?? string.Empty;
        createAccount.AccountName = createAccount.AccountName?.Trim() ?? string.Empty;
        createAccount.InitialDeposit = createAccount.InitialDeposit?.Trim() ?? string.Empty;
        createAccount.TargetAmount = createAccount.TargetAmount?.Trim() ?? string.Empty;
        createAccount.SelectedFrequency = string.IsNullOrWhiteSpace(createAccount.SelectedFrequency)
            ? "OneTime"
            : createAccount.SelectedFrequency.Trim();

        if (!createAccount.FundingSourceId.HasValue && fundingSources.Count > 0)
        {
            createAccount.FundingSourceId = fundingSources[0].Id;
        }

        if (!IsGoalSavings(createAccount.SelectedSavingsType))
        {
            createAccount.TargetAmount = string.Empty;
            createAccount.TargetDate = null;
        }

        if (!IsFixedDeposit(createAccount.SelectedSavingsType))
        {
            createAccount.MaturityDate = null;
        }
    }

    private static bool IsGoalSavings(string? savingsType) =>
        string.Equals(savingsType, "GoalSavings", StringComparison.OrdinalIgnoreCase);

    private static bool IsFixedDeposit(string? savingsType) =>
        string.Equals(savingsType, "FixedDeposit", StringComparison.OrdinalIgnoreCase);
}
