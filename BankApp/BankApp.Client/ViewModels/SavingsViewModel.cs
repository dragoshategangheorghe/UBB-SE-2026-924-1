namespace BankApp.Client.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Client.Utilities;
    using BankApp.Models.DTOs.Savings;
    using BankApp.Models.Entities;
    using BankApp.Models.Features.Investments;
    using BankApp.Models.Features.Savings;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using BankApp.Models.Enums;

    public partial class SavingsViewModel : BaseViewModel
    {
        private const int InitialPage = 1;
        private const int DefaultTransactionPageSize = 10;
        private const int InitialAutoDepositDelayDays = 1;
        private const decimal ZeroAmount = 0m;
        private const string OneTimeFrequency = "OneTime";

        private readonly ISavingsService savingsService;

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
        private ObservableCollection<SavingsAccount> closeDestinationAccounts = new ObservableCollection<SavingsAccount>();

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
        private string depositAmountText = string.Empty;

        private CancellationTokenSource? depositCancelationTokenSource;

        [ObservableProperty]
        private string depositSource = string.Empty;
        [ObservableProperty]
        private string depositSuccessMessage = string.Empty;
        [ObservableProperty]
        private ObservableCollection<FundingSourceOption> fundingSources = new ObservableCollection<FundingSourceOption>();
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
        private ObservableCollection<SavingsAccount> savingsAccounts = new ObservableCollection<SavingsAccount>();

        [ObservableProperty]
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
        private ObservableCollection<SavingsTransaction> transactions = new ObservableCollection<SavingsTransaction>();

        [ObservableProperty]
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
        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string livePreview = string.Empty;

        [ObservableProperty]
        private bool withdrawHasEarlyRisk;

        [ObservableProperty]
        private bool withdrawHasPenalty;

        [ObservableProperty]
        private decimal withdrawEstimatedPenalty;

        [ObservableProperty]
        private decimal withdrawNetAmount;

        [ObservableProperty]
        private string withdrawPenaltyBreakdownText = string.Empty;

        [ObservableProperty]
        private string withdrawNetAmountText = string.Empty;

        [ObservableProperty]
        private string withdrawPenaltySummary = string.Empty;

        [ObservableProperty]
        private bool closeHasPenalty;

        private string _pendingTargetAmountText = string.Empty;

        private bool _isBusy;

        // ── Constructor ──────────────────────────────────────────────────────
        public SavingsViewModel(ISavingsService savingsService)
        {
            this.savingsService = savingsService ?? throw new ArgumentNullException(nameof(savingsService));
        }

        public bool IsEmpty => !this.SavingsAccounts.Any();

        public bool ShowAccountsList => this.SavingsAccounts.Any();

        public bool IsGoalSavings => this.SelectedSavingsType == "GoalSavings";

        public bool IsFixedDeposit => this.SelectedSavingsType == "FixedDeposit";

        public Dictionary<string, string> FieldErrors { get; } = new Dictionary<string, string>();

        public string ExistingLabel => this.HasExistingAutoDeposit ? "Modify" : "Set Up";

        public DateTimeOffset? MaturityDate { get; set; }

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

        partial void OnSelectedAccountChanged(SavingsAccount? value)
        {
            _ = RefreshSelectedAccountDependentsAsync();
        }

        partial void OnDepositAmountTextChanged(string value)
        {
            _ = RefreshDepositPreviewAsync();
        }

        partial void OnWithdrawAmountTextChanged(string value)
        {
            _ = RefreshWithdrawPenaltyAsync();
        }

        partial void OnErrorMessageChanged(string value)
        {
            this.HasError = !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Refreshes all observable fields that depend on the currently selected account.
        /// Triggered via OnSelectedAccountChanged.
        /// </summary>
        private async Task RefreshSelectedAccountDependentsAsync()
        {
            if (_isBusy)
            {
                return;
            }
            if (this.SelectedAccount == null)
            {
                this.WithdrawHasEarlyRisk = false;
                this.WithdrawHasPenalty = false;
                this.WithdrawEstimatedPenalty = ZeroAmount;
                this.WithdrawNetAmount = ZeroAmount;
                this.WithdrawPenaltyBreakdownText = string.Empty;
                this.WithdrawNetAmountText = string.Empty;
                this.WithdrawPenaltySummary = string.Empty;
                this.CloseHasPenalty = false;
                this.LivePreview = string.Empty;
                return;
            }

            try
            {
                this.WithdrawHasEarlyRisk = await this.savingsService.HasRiskEarlyWithdrawal(this.SelectedAccount);
                this.CloseHasPenalty = await this.savingsService.CheckClosePenaltyRiskAsync(this.SelectedAccount);

                if (this.WithdrawHasEarlyRisk)
                {
                    var penaltyRate = await this.savingsService.GetPenaltyDecimalFor("EarlyWithdrawal");
                    this.WithdrawPenaltySummary =
                        $"Early withdrawal penalty: {penaltyRate:P2} of amount. Maturity date: {this.SelectedAccount.MaturityDate:d}";
                }
                else
                {
                    this.WithdrawPenaltySummary = string.Empty;
                }

                // Re-run amount-dependent refresh in case an amount was already typed.
                await this.RefreshWithdrawPenaltyAsync();
                await this.RefreshDepositPreviewAsync();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Refreshes the deposit live-preview label.
        /// Triggered via OnDepositAmountTextChanged and after account selection.
        /// </summary>
        private async Task RefreshDepositPreviewAsync()
        {
            if (_isBusy)
            {
                return;
            }
            if (this.SelectedAccount == null || string.IsNullOrWhiteSpace(this.DepositAmountText))
            {
                this.LivePreview = string.Empty;
                return;
            }

            try
            {
                this.LivePreview = await this.savingsService.GetDepositPreviewAsync(
                    this.DepositAmountText,
                    this.SelectedAccount);
            }
            catch
            {
                this.LivePreview = string.Empty;
            }
        }

        /// <summary>
        /// Refreshes all withdraw penalty fields based on the current amount text.
        /// Triggered via OnWithdrawAmountTextChanged and after account selection.
        /// </summary>
        private async Task RefreshWithdrawPenaltyAsync()
        {
            if (_isBusy)
            {
                return;
            }
            if (!this.WithdrawHasEarlyRisk)
            {
                this.WithdrawEstimatedPenalty = ZeroAmount;
                this.WithdrawNetAmount = ZeroAmount;
                this.WithdrawHasPenalty = false;
                this.WithdrawPenaltyBreakdownText = string.Empty;
                this.WithdrawNetAmountText = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(this.WithdrawAmountText))
            {
                this.WithdrawEstimatedPenalty = ZeroAmount;
                this.WithdrawNetAmount = ZeroAmount;
                this.WithdrawHasPenalty = false;
                this.WithdrawPenaltyBreakdownText = string.Empty;
                this.WithdrawNetAmountText = string.Empty;
                return;
            }

            decimal amount;
            try
            {
                amount = await this.savingsService.ParsePositiveAmountAsync(this.WithdrawAmountText);
            }
            catch (InvalidOperationException)
            {
                // Invalid/empty text — reset penalty fields silently.
                this.WithdrawEstimatedPenalty = ZeroAmount;
                this.WithdrawNetAmount = ZeroAmount;
                this.WithdrawHasPenalty = false;
                this.WithdrawPenaltyBreakdownText = string.Empty;
                this.WithdrawNetAmountText = string.Empty;
                return;
            }

            try
            {
                var penaltyRate = await this.savingsService.GetPenaltyDecimalFor("EarlyWithdrawal");
                this.WithdrawEstimatedPenalty = await this.savingsService.ComputeWithdrawalPenalty(amount);
                this.WithdrawNetAmount = await this.savingsService.GetWithdrawNetAmountAsync(amount, this.WithdrawEstimatedPenalty);
                this.WithdrawHasPenalty = this.WithdrawEstimatedPenalty > ZeroAmount;
                this.WithdrawPenaltyBreakdownText = $"Penalty ({penaltyRate:P0}): -${this.WithdrawEstimatedPenalty:N2}";
                this.WithdrawNetAmountText = $"Net amount received: ${this.WithdrawNetAmount:N2}";
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
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

                this.SelectedFundingSource = await this.savingsService.GetDefaultFundingSourceAsync(this.FundingSources);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void PrepareCreateAccountSubmission(
            string selectedSavingsType,
            string selectedFrequency,
            string accountName,
            string initialDepositText,
            FundingSourceOption? fundingSource,
            string targetAmountText,
            DateTimeOffset? targetDate,
            DateTimeOffset? maturityDate)
        {
            this.SelectedSavingsType = selectedSavingsType;
            this.SelectedFrequency = string.IsNullOrWhiteSpace(selectedFrequency)
                ? OneTimeFrequency
                : selectedFrequency;
            this.AccountName = accountName;
            this.InitialDepositText = initialDepositText;
            this.SelectedFundingSource = fundingSource;
            this.TargetAmount = null;

            this._pendingTargetAmountText = targetAmountText;

            this.TargetDate = this.IsGoalSavings ? targetDate : null;
            this.MaturityDate = this.SelectedSavingsType == "FixedDeposit" ? maturityDate : null;
        }

        private async Task<bool> ValidationCreateAccount()
        {
            if (string.IsNullOrWhiteSpace(this.SelectedFrequency))
            {
                this.SelectedFrequency = OneTimeFrequency;
            }

            if (this.IsGoalSavings && !string.IsNullOrWhiteSpace(this._pendingTargetAmountText))
            {
                try
                {
                    this.TargetAmount = await this.savingsService
                        .ParsePositiveAmountAsync(this._pendingTargetAmountText);
                }
                catch (InvalidOperationException)
                {
                    this.TargetAmount = null;
                }
            }

            var errors = await this.savingsService.ValidateCreateAccountAsync(new ValidateCreateAccountRequest
            {
                SelectedSavingsType = this.SelectedSavingsType,
                AccountName = this.AccountName,
                InitialDepositText = this.InitialDepositText,
                HasFundingSource = this.SelectedFundingSource != null,
                SelectedFrequency = this.SelectedFrequency,
                TargetAmount = this.TargetAmount,
                TargetDate = this.TargetDate,
                IsGoalSavings = this.IsGoalSavings,
            });

            foreach (var error in errors)
            {
                this.FieldErrors[error.Key] = error.Value;
            }

            this.OnPropertyChanged(nameof(this.FieldErrors));

            return !this.FieldErrors.Any();
        }

        private async Task ExecuteCreateAccountAsync()
        {
            this.IsLoading = true;
            try
            {
                var deposit = await this.savingsService.ParsePositiveAmountAsync(this.InitialDepositText);

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
                    DepositFrequency = string.IsNullOrWhiteSpace(this.SelectedFrequency) ||
                                       string.Equals(this.SelectedFrequency, OneTimeFrequency, StringComparison.OrdinalIgnoreCase)
                        ? null
                        : await this.savingsService.ParseDepositFrequencyAsync(this.SelectedFrequency),
                };

                await this.savingsService.CreateAccountAsync(createSavingsAccountDto);

                this.ShowCreateConfirmation = true;
                this.ResetCreateForm();
                await this.LoadAccountsAsync();
            }
            catch (Exception exception) when (
            exception is InvalidOperationException
            || exception is ArgumentException)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task CreateAccountAsync()
        {
            if (_isBusy)
            {
                return;
            }
            _isBusy = true;

            try
            {
                this.FieldErrors.Clear();
                this.ErrorMessage = string.Empty;
                this.ShowCreateConfirmation = false;

                if (!await ValidationCreateAccount())
                {
                    return;
                }

                await ExecuteCreateAccountAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void ResetCreateForm()
        {
            this.AccountName = string.Empty;
            this.InitialDepositText = string.Empty;
            this.SelectedSavingsType = string.Empty;
            this.SelectedFrequency = OneTimeFrequency;
            this.TargetAmount = null;
            this.TargetDate = null;
            this.MaturityDate = null;
            this._pendingTargetAmountText = string.Empty;
            this.FieldErrors.Clear();
        }

        // ── Commands: Deposit ────────────────────────────────────────────────
        [RelayCommand]
        public async Task DepositAsync()
        {
            if (_isBusy)
            {
                return;
            }
            _isBusy = true;

            try
            {
                this.ErrorMessage = string.Empty;
                this.ShowDepositSuccess = false;

                if (this.SelectedAccount == null)
                {
                    this.ErrorMessage = "No account selected.";
                    return;
                }

                decimal amount;
                try
                {
                    amount = await this.savingsService.ParsePositiveAmountAsync(this.DepositAmountText);
                }
                catch
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
                catch (InvalidOperationException exception)
                {
                    this.ErrorMessage = exception.Message;
                }
                finally
                {
                    this.IsLoading = false;
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        public void CancelDeposit()
        {
            this.depositCancelationTokenSource?.Cancel();
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

                this.TotalSavedAmount = await this.savingsService.GetTotalSavedAmountAsync(this.SavingsAccounts);
                this.NumberOfAccountsText =
                    await this.savingsService.GetNumberOfAccountsTextAsync(this.SavingsAccounts.Count);
                this.BestInterestRate = await this.savingsService.GetBestInterestRateAsync(this.SavingsAccounts);
            }
            catch (Exception exception) when (
            exception is ArgumentException
            || exception is InvalidOperationException)
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
            var openAccountsList = await this.savingsService.GetValidTransferDestinationsAsync(
                this.SelectedAccount!.IdentificationNumber,
                this.CurrentUser.Id);
            this.CloseDestinationAccounts.Clear();
            foreach (var account in openAccountsList)
            {
                this.CloseDestinationAccounts.Add(account);
            }

            this.SelectedCloseDestinationId =
                await this.savingsService.GetDefaultCloseDestinationIdAsync(this.CloseDestinationAccounts);
            this.OnPropertyChanged(nameof(this.CloseHasPenalty));
        }

        public async Task<bool> ConfirmCloseAsync()
        {
            var closeValidation = await this.savingsService.ValidateCloseConfirmationAsync(
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
            catch (InvalidOperationException exception)
            {
                this.CloseResultMessage = exception.Message;
                return false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private async Task<(bool IsValid, decimal Amount)> ValidateWithdrawInputAsync()
        {
            try
            {
                decimal amount = await this.savingsService.ParsePositiveAmountAsync(this.WithdrawAmountText);

                var validation = await this.savingsService.ValidateWithdrawRequestAsync(amount, this.WithdrawDestination);

                if (!validation.IsValid)
                {
                    this.WithdrawResultMessage = validation.ErrorMessage;
                    return (false, 0);
                }
                return (true, amount);
            }
            catch
            {
                this.WithdrawResultMessage = "Please enter a valid positive amount.";
                return (false, 0);
            }
        }

        private async Task<bool> RunWithdrawalTransactionAsync(decimal amount)
        {
            this.IsLoading = true;
            try
            {
                var response = await this.savingsService.WithdrawAsync(
                    this.SelectedAccount!.IdentificationNumber,
                    amount,
                    this.WithdrawDestination.DisplayName,
                    CurrentUser.Id);
                this.WithdrawSuccess = response.Success;
                this.WithdrawResultMessage = await this.savingsService.BuildWithdrawResultMessageAsync(response);
                if (response.Success)
                {
                    this.WithdrawAmountText = string.Empty;
                    await this.LoadAccountsAsync();
                }
                return response.Success;
            }
            catch (ArgumentException exception)
            {
                this.WithdrawResultMessage = exception.Message;
                return false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public async Task<bool> ConfirmWithdrawAsync()
        {
            if (_isBusy)
            {
                return false;
            }
            _isBusy = true;
            try
            {
                this.WithdrawResultMessage = string.Empty;

                var (isValid, amount) = await ValidateWithdrawInputAsync();

                if (!isValid)
                {
                    return false;
                }

                return await RunWithdrawalTransactionAsync(amount);
            }
            finally
            {
                _isBusy = false;
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

            decimal amount;
            try
            {
                amount = await this.savingsService.ParsePositiveAmountAsync(this.AutoDepositAmountText);
            }
            catch (InvalidOperationException)
            {
                this.ErrorMessage = "Auto deposit amount must be positive.";
                return;
            }

            if (string.IsNullOrWhiteSpace(this.AutoDepositFrequency))
            {
                this.ErrorMessage = "Please select a frequency.";
                return;
            }

            DepositFrequency frequency;
            try
            {
                frequency = await this.savingsService.ParseDepositFrequencyAsync(this.AutoDepositFrequency);
            }
            catch (InvalidOperationException)
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
                SourceAccountId = this.SelectedAccount.FundingAccount?.Id,
            };

            await this.savingsService.SaveAutoDepositAsync(autoDeposit);
            this.AutoDepositSaveMessage = "Auto deposit saved successfully.";
            await this.LoadAutoDepositAsync(this.SelectedAccount.IdentificationNumber);
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

                this.totalPages = await this.savingsService.GetTotalPagesAsync(result.TotalCount, DefaultTransactionPageSize);
            }
            catch (Exception exception) when (
            exception is InvalidOperationException
            || exception is ArgumentException)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public async Task NextPage(int accountId)
        {
            if (!await this.savingsService.CanMoveToNextPageAsync(this.currentPage, this.totalPages))
            {
                return;
            }

            this.currentPage++;
            await this.LoadTransactionsAsync(accountId);
        }

        public async Task PreviousPage(int accountId)
        {
            if (!await this.savingsService.CanMoveToPreviousPageAsync(this.currentPage))
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
            this.transactions.Clear();

            this.SavingsAccounts.Clear();
            this.CloseDestinationAccounts.Clear();
            this.FundingSources.Clear();

            this.depositCancelationTokenSource?.Cancel();
            this.depositCancelationTokenSource?.Dispose();
        }
    }
}
