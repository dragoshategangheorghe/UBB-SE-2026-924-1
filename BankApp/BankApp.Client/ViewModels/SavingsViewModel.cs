namespace BankApp.Client.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BankApp.Client.Utilities;
    using BankApp.Models.DTOs.Savings;
    using BankApp.Models.Entities;
    using BankApp.Models.Features.Investments;
    using BankApp.Models.Features.Savings;
    using BankApp.Server.Services.Implementations;
    using BankApp.Server.Services.Interfaces;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class SavingsViewModel : BaseViewModel
    {
        private const int InitialPage = 1;
        private const int DefaultTransactionPageSize = 10;
        private const int InitialAutoDepositDelayDays = 1;
        private const decimal ZeroAmount = 0m;

        private readonly SavingsPresentationService savingsPresentationService;
        private readonly ISavingsService savingsService;
        private readonly SavingsUiRulesService savingsUiRulesService;
        private readonly SavingsWorkflowService savingsWorkflowService;

        [ObservableProperty]
        private string accountName = string.Empty;

        [ObservableProperty]
        private string autoDepositAmountText = string.Empty;
        [ObservableProperty]
        private string autoDepositFrequency = string.Empty;
        [ObservableProperty]
        private bool autoDepositIsActive = true;
        [ObservableProperty]
        private string autoDepositSaveMessage = string.Empty;
        [ObservableProperty]
        private DateTimeOffset? autoDepositStartDate = DateTimeOffset.Now.AddDays(InitialAutoDepositDelayDays);
        [ObservableProperty]
        private string bestInterestRate = string.Empty;

        // ── Close Account Panel ──────────────────────────────────────────────
        [ObservableProperty]
        private ObservableCollection<SavingsAccount> closeDestinationAccounts = new();

        [ObservableProperty]
        private string closeResultMessage = string.Empty;
        [ObservableProperty]
        private bool closeSuccess;

        private bool closeUserConfirmed;

        // ── Auto Deposit ─────────────────────────────────────────────────────
        private AutoDeposit? currentAutoDeposit;

        [ObservableProperty]
        private int currentPage = InitialPage;

        // ── Deposit ──────────────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LivePreview))]
        private string depositAmountText = string.Empty;

        private CancellationTokenSource? depositCancelationTokenSource;

        [ObservableProperty]
        private string depositSource = string.Empty;
        [ObservableProperty]
        private string depositSuccessMessage = string.Empty;
        [ObservableProperty]
        private ObservableCollection<FundingSourceOption> fundingSources = new();
        [ObservableProperty]
        private bool hasExistingAutoDeposit;
        [ObservableProperty]
        private string initialDepositText = string.Empty;
        [ObservableProperty]
        private string numberOfAccountsText = string.Empty;

        // ── My Accounts ──────────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        [NotifyPropertyChangedFor(nameof(ShowAccountsList))]
        private ObservableCollection<SavingsAccount> savingsAccounts = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LivePreview))]
        [NotifyPropertyChangedFor(nameof(WithdrawHasEarlyRisk))]
        [NotifyPropertyChangedFor(nameof(WithdrawPenaltySummary))]
        [NotifyPropertyChangedFor(nameof(WithdrawEstimatedPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmount))]
        [NotifyPropertyChangedFor(nameof(WithdrawHasPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawPenaltyBreakdownText))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmountText))]
        [NotifyPropertyChangedFor(nameof(CloseHasPenalty))]
        private SavingsAccount? selectedAccount;

        private int selectedCloseDestinationId;

        [ObservableProperty] private string selectedFilter = "All";

        [ObservableProperty] private string selectedFrequency = string.Empty;
        [ObservableProperty] private FundingSourceOption? selectedFundingSource;

        // ── Create Account ───────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGoalSavings))]
        [NotifyPropertyChangedFor(nameof(IsFixedDeposit))]
        private string selectedSavingsType = string.Empty;

        [ObservableProperty]
        private bool showCreateConfirmation;
        [ObservableProperty]
        private bool showDepositSuccess;
        [ObservableProperty]
        private decimal? targetAmount;
        [ObservableProperty]
        private DateTimeOffset? targetDate;

        [ObservableProperty]
        private int totalPages;

        [ObservableProperty]
        private string totalSavedAmount = string.Empty;

        [ObservableProperty]
        private ObservableCollection<SavingsTransaction> transactions = new();

        // ── Withdraw Panel ───────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WithdrawEstimatedPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmount))]
        [NotifyPropertyChangedFor(nameof(WithdrawHasPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawPenaltyBreakdownText))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmountText))]
        private string withdrawAmountText = string.Empty;

        [ObservableProperty]
        private FundingSourceOption? withdrawDestination;
        [ObservableProperty]
        private string withdrawResultMessage = string.Empty;
        [ObservableProperty]
        private bool withdrawSuccess;
        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private User currentUser;
        [ObservableProperty]
        private string errorMessage;

        // ── Constructor ──────────────────────────────────────────────────────
        public SavingsViewModel(ISavingsService savingsService)
        {
            this.savingsService = savingsService;
            this.savingsUiRulesService = new SavingsUiRulesService();
            this.savingsPresentationService = new SavingsPresentationService();
            this.savingsWorkflowService = new SavingsWorkflowService();
        }

        public bool IsEmpty => !this.SavingsAccounts.Any();

        public bool ShowAccountsList => this.SavingsAccounts.Any();

        public bool IsGoalSavings => this.SelectedSavingsType == "GoalSavings";

        public bool IsFixedDeposit => this.SelectedSavingsType == "FixedDeposit";

        public Dictionary<string, string> FieldErrors { get; } = new();

        public string LivePreview =>
            this.savingsUiRulesService.BuildDepositPreview(this.DepositAmountText, this.SelectedAccount);

        public bool WithdrawHasEarlyRisk => this.savingsService.HasRiskEarlyWithdrawal(this.SelectedAccount);

        public decimal WithdrawEstimatedPenalty
        {
            get
            {
                if (!this.WithdrawHasEarlyRisk)
                {
                    return ZeroAmount;
                }

                if (!this.savingsUiRulesService.TryParsePositiveAmount(this.WithdrawAmountText, out var withdrawAmount))
                {
                    return ZeroAmount;
                }

                return this.savingsService.ComputeWithdrawalPenalty(withdrawAmount);
            }
        }

        public decimal WithdrawNetAmount
        {
            get
            {
                if (!this.savingsUiRulesService.TryParsePositiveAmount(this.WithdrawAmountText, out var withdrawAmount))
                {
                    return ZeroAmount;
                }

                return this.savingsUiRulesService.CalculateWithdrawNetAmount(withdrawAmount, this.WithdrawEstimatedPenalty);
            }
        }

        public bool WithdrawHasPenalty => this.WithdrawEstimatedPenalty > ZeroAmount;

        public string WithdrawPenaltyBreakdownText =>
            $"Penalty ({this.savingsService.GetPenaltyDecimalFor("EarlyWithdrawal"):P0}): -${this.WithdrawEstimatedPenalty:N2}";

        public string WithdrawNetAmountText => $"Net amount received: ${this.WithdrawNetAmount:N2}";

        public string WithdrawPenaltySummary => this.WithdrawHasEarlyRisk
            ? $"Early withdrawal penalty: {this.savingsService.GetPenaltyDecimalFor("EarlyWithdrawal"):P2} of amount. Maturity date: {this.SelectedAccount?.MaturityDate:d}"
            : string.Empty;

        public string ExistingLabel => this.HasExistingAutoDeposit ? "Modify" : "Set Up";

        public int SelectedCloseDestinationId
        {
            get => this.selectedCloseDestinationId;
            set
            {
                this.selectedCloseDestinationId = value;
                this.OnPropertyChanged();
            }
        }

        public bool CloseUserConfirmed
        {
            get => this.closeUserConfirmed;
            set
            {
                this.closeUserConfirmed = value;
                this.OnPropertyChanged();
            }
        }

        public bool CloseHasPenalty => this.savingsPresentationService.HasClosePenaltyRisk(this.SelectedAccount);

        public DateTimeOffset? MaturityDate { get; set; }

        public async Task<bool> ConfirmWithdrawAsync()
        {
            this.WithdrawResultMessage = string.Empty;
            this.WithdrawSuccess = false;
            this.savingsUiRulesService.TryParsePositiveAmount(this.WithdrawAmountText, out var amount);
            var withdrawValidation = this.savingsWorkflowService.ValidateWithdrawRequest(amount, this.WithdrawDestination);
            if (!withdrawValidation.IsValid)
            {
                this.WithdrawResultMessage = withdrawValidation.ErrorMessage;
                return false;
            }

            this.IsLoading = true;
            try
            {
                var withdrawResponseDto = await this.savingsService.WithdrawAsync(
                    this.SelectedAccount!.IdentificationNumber,
                    amount,
                    this.WithdrawDestination.DisplayName,
                    CurrentUser.Id);
                this.WithdrawSuccess = withdrawResponseDto.Success;
                this.WithdrawResultMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(withdrawResponseDto);
                if (withdrawResponseDto.Success)
                {
                    this.WithdrawAmountText = string.Empty;
                    await this.LoadAccountsAsync();
                }

                return withdrawResponseDto.Success;
            }
            catch (Exception exception)
            {
                this.WithdrawResultMessage = exception.Message;
                return false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public async Task LoadAutoDepositAsync(int accountId)
        {
            this.AutoDepositSaveMessage = string.Empty;
            this.currentAutoDeposit = await this.savingsService.GetAutoDepositAsync(accountId);
            if (this.currentAutoDeposit != null)
            {
                this.HasExistingAutoDeposit = true;
                this.AutoDepositAmountText = this.currentAutoDeposit.Amount.ToString(CultureInfo.InvariantCulture);
                this.AutoDepositFrequency = this.currentAutoDeposit.Frequency.ToString();
                this.AutoDepositStartDate = new DateTimeOffset(this.currentAutoDeposit.NextRunDate);
                this.AutoDepositIsActive = this.currentAutoDeposit.IsActive;
            }
            else
            {
                this.HasExistingAutoDeposit = false;
                this.AutoDepositAmountText = string.Empty;
                this.AutoDepositFrequency = string.Empty;
                this.AutoDepositStartDate = DateTimeOffset.Now.AddDays(InitialAutoDepositDelayDays);
                this.AutoDepositIsActive = true;
            }
        }

        public async Task SaveAutoDepositAsync()
        {
            this.ErrorMessage = string.Empty;
            this.AutoDepositSaveMessage = string.Empty;

            if (!this.savingsUiRulesService.TryParsePositiveAmount(this.AutoDepositAmountText, out var amount))
            {
                this.ErrorMessage = "Auto deposit amount must be positive.";
                return;
            }

            if (string.IsNullOrWhiteSpace(this.AutoDepositFrequency))
            {
                this.ErrorMessage = "Please select a frequency.";
                return;
            }

            if (!this.savingsUiRulesService.TryParseDepositFrequency(this.AutoDepositFrequency, out var frequency))
            {
                this.ErrorMessage = "Invalid frequency.";
                return;
            }

            var autoDeposit = new AutoDeposit
            {
                Id = this.currentAutoDeposit?.Id ?? default,
                SavingsAccountId = this.SelectedAccount!.IdentificationNumber,
                Amount = amount,
                Frequency = frequency,
                NextRunDate = this.AutoDepositStartDate?.DateTime ?? DateTime.Now.AddDays(InitialAutoDepositDelayDays),
                IsActive = this.AutoDepositIsActive,
            };

            await this.savingsService.SaveAutoDepositAsync(autoDeposit);
            this.AutoDepositSaveMessage = "Auto deposit saved successfully.";
            await this.LoadAutoDepositAsync(this.SelectedAccount.IdentificationNumber);
        }

        // ── Commands: My Accounts ────────────────────────────────────────────
        [RelayCommand]
        public async Task LoadAccountsAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                var accountsList = await this.savingsService.GetAccountsAsync(CurrentUser.Id);
                this.SavingsAccounts.Clear();
                foreach (var account in accountsList)
                {
                    this.SavingsAccounts.Add(account);
                }

                this.OnPropertyChanged(nameof(this.IsEmpty));
                this.OnPropertyChanged(nameof(this.ShowAccountsList));

                this.TotalSavedAmount = this.savingsPresentationService.BuildTotalSavedAmount(this.SavingsAccounts);
                this.NumberOfAccountsText =
                    this.savingsPresentationService.BuildNumberOfAccountsText(this.SavingsAccounts.Count);
                this.BestInterestRate = this.savingsPresentationService.BuildBestInterestRate(this.SavingsAccounts);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task CloseAccountAsync(SavingsAccount account)
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                var closureResultDto = await this.savingsService.CloseAccountAsync(
                    account.IdentificationNumber,
                    this.SelectedCloseDestinationId,
                    CurrentUser.Id);
                var ok = closureResultDto.Success;
                if (!ok)
                {
                    this.ErrorMessage = "Failed to close account.";
                    return;
                }

                await this.LoadAccountsAsync();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public async Task LoadCloseDestinationAccountsAsync()
        {
            this.CloseUserConfirmed = false;
            this.CloseResultMessage = string.Empty;
            this.CloseSuccess = false;
            var openAccountsList = await this.savingsService.GetValidTransferDestinationsAsync(this.SelectedAccount!.IdentificationNumber);
            this.CloseDestinationAccounts.Clear();
            foreach (var account in openAccountsList)
            {
                this.CloseDestinationAccounts.Add(account);
            }

            this.SelectedCloseDestinationId =
                this.savingsWorkflowService.GetDefaultCloseDestinationId(this.CloseDestinationAccounts);
            this.OnPropertyChanged(nameof(this.CloseHasPenalty));
        }

        public async Task<bool> ConfirmCloseAsync()
        {
            var closeValidation = this.savingsWorkflowService.ValidateCloseConfirmation(
                this.CloseUserConfirmed,
                this.SelectedCloseDestinationId);
            if (!closeValidation.IsValid)
            {
                this.CloseResultMessage = closeValidation.ErrorMessage;
                return false;
            }

            this.IsLoading = true;
            try
            {
                var result = await this.savingsService.CloseAccountAsync(
                    this.SelectedAccount!.IdentificationNumber,
                    this.SelectedCloseDestinationId,
                    CurrentUser.Id);
                this.CloseSuccess = result.Success;
                this.CloseResultMessage = result.Success ? "Account closed successfully." : result.Message;
                if (result.Success)
                {
                    await this.LoadAccountsAsync();
                }

                return result.Success;
            }
            catch (Exception exception)
            {
                this.CloseResultMessage = exception.Message;
                return false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        // ── Commands: Create Account ─────────────────────────────────────────
        public async Task LoadFundingSourcesAsync()
        {
            try
            {
                var fundingSourcesList = await this.savingsService.GetFundingSourcesAsync(CurrentUser.Id);
                this.FundingSources.Clear();
                foreach (var fundingSource in fundingSourcesList)
                {
                    this.FundingSources.Add(fundingSource);
                }

                this.SelectedFundingSource = this.savingsWorkflowService.GetDefaultFundingSource(this.FundingSources);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void PrepareCreateAccountSubmission(
            string accountName,
            string initialDepositText,
            FundingSourceOption? fundingSource,
            string targetAmountText,
            DateTimeOffset? targetDate,
            DateTimeOffset? maturityDate)
        {
            this.AccountName = accountName;
            this.InitialDepositText = initialDepositText;
            this.SelectedFundingSource = fundingSource;
            this.TargetAmount = null;

            if (this.IsGoalSavings &&
                this.savingsUiRulesService.TryParsePositiveAmount(targetAmountText, out var parsedTargetAmount))
            {
                this.TargetAmount = parsedTargetAmount;
            }

            this.TargetDate = this.IsGoalSavings ? targetDate : null;
            this.MaturityDate = this.SelectedSavingsType == "FixedDeposit" ? maturityDate : null;
        }

        [RelayCommand]
        public async Task CreateAccountAsync()
        {
            this.FieldErrors.Clear();
            this.ErrorMessage = string.Empty;
            this.ShowCreateConfirmation = false;

            var errors = this.savingsUiRulesService.ValidateCreateAccount(
                this.SelectedSavingsType,
                this.AccountName,
                this.InitialDepositText,
                this.SelectedFundingSource != null,
                this.SelectedFrequency,
                this.TargetAmount,
                this.TargetDate,
                this.IsGoalSavings);

            foreach (var error in errors)
            {
                this.FieldErrors[error.Key] = error.Value;
            }

            this.OnPropertyChanged(nameof(this.FieldErrors));
            if (this.FieldErrors.Any())
            {
                return;
            }

            this.savingsUiRulesService.TryParsePositiveAmount(this.InitialDepositText, out var deposit);

            this.IsLoading = true;
            try
            {
                var createSavingsAccountDto = new CreateSavingsAccountDto
                {
                    UserIdentificationNumber = CurrentUser.Id,
                    SavingsType = this.SelectedSavingsType,
                    AccountName = this.AccountName.Trim(),
                    InitialDeposit = deposit,
                    FundingAccountId = this.SelectedFundingSource!.Id,
                    TargetAmount = this.IsGoalSavings ? this.TargetAmount : null,
                    TargetDate = this.IsGoalSavings ? this.TargetDate?.DateTime : null,
                    MaturityDate = this.MaturityDate?.DateTime,
                    DepositFrequency =
                        this.savingsUiRulesService.TryParseDepositFrequency(
                            this.SelectedFrequency,
                            out var selectedFrequency)
                            ? selectedFrequency
                            : null,
                };
                await this.savingsService.CreateAccountAsync(createSavingsAccountDto);
                this.ShowCreateConfirmation = true;
                this.ResetCreateForm();
                await this.LoadAccountsAsync();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private void ResetCreateForm()
        {
            this.AccountName = string.Empty;
            this.InitialDepositText = string.Empty;
            this.SelectedSavingsType = string.Empty;
            this.TargetAmount = null;
            this.TargetDate = null;
            this.FieldErrors.Clear();
        }

        // ── Commands: Deposit ────────────────────────────────────────────────
        [RelayCommand]
        public async Task DepositAsync()
        {
            this.ErrorMessage = string.Empty;
            this.ShowDepositSuccess = false;

            if (this.SelectedAccount == null)
            {
                this.ErrorMessage = "No account selected.";
                return;
            }

            if (!this.savingsUiRulesService.TryParsePositiveAmount(this.DepositAmountText, out var amount))
            {
                this.ErrorMessage = "Please enter a valid positive amount.";
                return;
            }

            this.depositCancelationTokenSource?.Cancel();
            this.depositCancelationTokenSource = new CancellationTokenSource();

            this.IsLoading = true;
            try
            {
                var depositResponseDto = await this.savingsService.DepositAsync(
                    this.SelectedAccount.IdentificationNumber,
                    amount,
                    this.DepositSource,
                    CurrentUser.Id);

                this.DepositSuccessMessage = $"Deposit successful! New balance: ${depositResponseDto.NewBalance:N2}";
                this.ShowDepositSuccess = true;
                this.DepositAmountText = string.Empty;
                await this.LoadAccountsAsync();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public void CancelDeposit()
        {
            this.depositCancelationTokenSource?.Cancel();
        }

        public async Task LoadTransactionsAsync(int accountId)
        {
            try
            {
                var result = await this.savingsService.GetTransactionsAsync(
                    accountId,
                    this.selectedFilter,
                    this.currentPage,
                    DefaultTransactionPageSize);

                this.transactions.Clear();

                foreach (var transaction in result.Items)
                {
                    this.transactions.Add(transaction);
                }

                this.totalPages = this.savingsUiRulesService.CalculateTotalPages(result.TotalCount, DefaultTransactionPageSize);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public async Task NextPage(int accountId)
        {
            if (!this.savingsWorkflowService.CanMoveToNextPage(this.currentPage, this.totalPages))
            {
                return;
            }

            this.currentPage++;
            await this.LoadTransactionsAsync(accountId);
        }

        public async Task PreviousPage(int accountId)
        {
            if (!this.savingsWorkflowService.CanMoveToPreviousPage(this.currentPage))
            {
                return;
            }

            this.currentPage--;
            await this.LoadTransactionsAsync(accountId);
        }

        public async Task ChangeFilter(int accountId, string filter)
        {
            this.selectedFilter = filter;
            this.currentPage = InitialPage;
            await this.LoadTransactionsAsync(accountId);
        }

        public override void Dispose()
        {
            // write the code for this method
            this.transactions.Clear();

            this.SavingsAccounts.Clear();
            this.CloseDestinationAccounts.Clear();
            this.FundingSources.Clear();

            this.depositCancelationTokenSource?.Cancel();
            this.depositCancelationTokenSource?.Dispose();
        }
    }
}