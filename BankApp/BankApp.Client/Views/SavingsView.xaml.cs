namespace BankApp.Client.Views
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BankApp.Client.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using BankApp.Models.Features.Investments;
    using BankApp.Models.Features.Savings;
    using BankApp.Models.Entities;

    public sealed partial class SavingsView : UserControl
    {
        private const int FirstItemIndex = 0;
        private const int NoSelectionIndex = -1;
        private const int SuccessMessageDelayMilliseconds = 1500;
        private const string OneTimeFrequencyTag = "OneTime";

        public SavingsViewModel? ViewModel => this.DataContext as SavingsViewModel;

        public SavingsView()
        {
            this.InitializeComponent();
            this.MainNavigationView.SelectedItem = this.MyAccountsTab;
            this.Loaded += SavingsView_Loaded;
        }

        private async void SavingsView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ne asigurăm că ViewModel-ul a fost injectat cu succes din XAML
            if (this.ViewModel != null)
            {
                try
                {
                    var userId = App.AuthService.GetCurrentUserId() ?? throw new Exception("Current user id is null.");
                    this.ViewModel.CurrentUser = new User { Id = userId };

                    await this.ViewModel.LoadAccountsAsync();
                }
                catch (Exception ex)
                {
                    this.ViewModel.ErrorMessage = ex.Message;
                }

                if (this.ViewModel.HasError)
                {
                    await this.ShowDialogAsync("Load Error", this.ViewModel.ErrorMessage);
                }
            }
        }

        // ── Tab switching ────────────────────────────────────────────────────
        private async void OnTabSelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavigationViewItem tab)
            {
                return;
            }

            var tag = tab.Tag?.ToString() ?? string.Empty;

            this.MyAccountsPanel.Visibility = tag == "MyAccounts" ? Visibility.Visible : Visibility.Collapsed;
            this.OpenNewPanel.Visibility = tag == "OpenNew" ? Visibility.Visible : Visibility.Collapsed;
            this.ManagePanel.Visibility = tag == "Manage" ? Visibility.Visible : Visibility.Collapsed;

            if (tag == "OpenNew")
            {
                this.ResetCreateFormControls();
                this.ViewModel.SelectedSavingsType = string.Empty;

                await this.ViewModel.LoadFundingSourcesAsync();
                this.FundingSourceComboBox.ItemsSource = this.ViewModel.FundingSources;
                if (this.ViewModel.FundingSources.Any())
                {
                    var defaultFundingSource = this.ViewModel.FundingSources
                        .FirstOrDefault(source => source.Id == this.ViewModel.SelectedFundingSource?.Id)
                        ?? this.ViewModel.FundingSources[FirstItemIndex];
                    this.FundingSourceComboBox.SelectedItem = defaultFundingSource;
                    this.ViewModel.SelectedFundingSource = defaultFundingSource;
                }
            }

            if (tag == "Manage")
            {
                this.HideAllActionPanels();
                this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
                this.ManageAccountComboBox.SelectedIndex = NoSelectionIndex;
            }
        }

        // ── Open New ─────────────────────────────────────────────────────────
        private void OnFrequencySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FrequencyRadioButtons.SelectedItem is RadioButton radioButton)
            {
                this.ViewModel.SelectedFrequency = radioButton.Tag?.ToString() ?? string.Empty;
            }
        }

        private void OnSavingsTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SavingsTypeRadioButtons.SelectedItem is RadioButton radioButton)
            {
                this.ViewModel.SelectedSavingsType = radioButton.Tag?.ToString() ?? string.Empty;

                this.GoalSavingsPanel.Visibility = this.ViewModel.SelectedSavingsType == "GoalSavings"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                this.FixedDepositPanel.Visibility = this.ViewModel.SelectedSavingsType == "FixedDeposit"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (this.ViewModel.SelectedSavingsType != "GoalSavings")
                {
                    this.TargetAmountTextBox.Text = string.Empty;
                    this.TargetDatePicker.Date = null;
                    this.TargetAmountError.Visibility = Visibility.Collapsed;
                    this.TargetDateError.Visibility = Visibility.Collapsed;
                }

                if (this.ViewModel.SelectedSavingsType != "FixedDeposit")
                {
                    this.MaturityDatePicker.Date = null;
                }
            }
        }

        private async void OnOpenAccountClicked(object sender, RoutedEventArgs e)
        {
            this.ClearCreateErrors();
            this.ViewModel.PrepareCreateAccountSubmission(
                this.GetSelectedRadioButtonTag(this.SavingsTypeRadioButtons),
                this.GetSelectedRadioButtonTag(this.FrequencyRadioButtons),
                this.AccountNameTextBox.Text,
                this.InitialDepositTextBox.Text,
                this.FundingSourceComboBox.SelectedItem as FundingSourceOption,
                this.TargetAmountTextBox.Text,
                this.TargetDatePicker.Date,
                this.MaturityDatePicker.Date);

            await this.ViewModel.CreateAccountCommand.ExecuteAsync(null);

            if (this.ViewModel.FieldErrors.TryGetValue("SavingsType", out var savingsTypeError))
            {
                ShowError(this.TypeErrorText, savingsTypeError);
            }

            if (this.ViewModel.FieldErrors.TryGetValue("AccountName", out var accountNameError))
            {
                ShowError(this.AccountNameError, accountNameError);
            }

            if (this.ViewModel.FieldErrors.TryGetValue("InitialDeposit", out var initialDepositError))
            {
                ShowError(this.InitialDepositError, initialDepositError);
            }

            if (this.ViewModel.FieldErrors.TryGetValue("FundingSource", out var fundingSourceError))
            {
                ShowError(this.FundingSourceError, fundingSourceError);
            }

            if (this.ViewModel.FieldErrors.TryGetValue("Frequency", out var frequencyError))
            {
                ShowError(this.FrequencyError, frequencyError);
            }

            if (this.ViewModel.FieldErrors.TryGetValue("TargetAmount", out var targetAmountError))
            {
                ShowError(this.TargetAmountError, targetAmountError);
            }

            if (this.ViewModel.FieldErrors.TryGetValue("TargetDate", out var targetDateError))
            {
                ShowError(this.TargetDateError, targetDateError);
            }

            if (this.ViewModel.HasError)
            {
                this.CreateErrorBar.Message = this.ViewModel.ErrorMessage;
                this.CreateErrorBar.IsOpen = true;
                return;
            }

            if (this.ViewModel.ShowCreateConfirmation)
            {
                this.CreateSuccessBar.IsOpen = true;
                this.OpenAccountButton.IsEnabled = false;
                await Task.Delay(SuccessMessageDelayMilliseconds);
                this.CreateSuccessBar.IsOpen = false;
                this.OpenAccountButton.IsEnabled = true;
                this.ResetCreateFormControls();
                this.MainNavigationView.SelectedItem = this.MyAccountsTab;
            }
        }

        private void OnCancelCreateClicked(object sender, RoutedEventArgs e)
        {
            this.MainNavigationView.SelectedItem = this.MyAccountsTab;
        }

        // ── Manage: account selection ────────────────────────────────────────
        private void OnManageAccountSelected(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedAccount = this.ManageAccountComboBox.SelectedItem as SavingsAccount;
            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = this.ViewModel.SelectedAccount != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ── Manage: show action panels ───────────────────────────────────────
        private async void OnDepositClicked(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedAccount == null)
            {
                return;
            }

            // Load funding sources into the deposit combobox
            await this.ViewModel.LoadFundingSourcesAsync();
            this.DepositSourceComboBox.ItemsSource = this.ViewModel.FundingSources;
            if (this.ViewModel.FundingSources.Any())
            {
                var defaultFundingSource = this.ViewModel.FundingSources
                    .FirstOrDefault(source => source.Id == this.ViewModel.SelectedFundingSource?.Id)
                    ?? this.ViewModel.FundingSources[FirstItemIndex];
                this.DepositSourceComboBox.SelectedItem = defaultFundingSource;
                this.ViewModel.DepositSource = defaultFundingSource.DisplayName;
            }

            // Sync amount field
            this.DepositAmountTextBox.Text = string.Empty;
            this.ViewModel.DepositAmountText = string.Empty;
            this.DepositLivePreview.Text = string.Empty;
            this.DepositResultBar.IsOpen = false;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.DepositActionPanel.Visibility = Visibility.Visible;
        }

        private async void OnWithdrawClicked(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedAccount == null)
            {
                return;
            }

            // Load funding sources as withdraw destinations
            await this.ViewModel.LoadFundingSourcesAsync();
            this.WithdrawDestComboBox.ItemsSource = this.ViewModel.FundingSources;
            if (this.ViewModel.FundingSources.Any())
            {
                var defaultFundingSource = this.ViewModel.FundingSources
                    .FirstOrDefault(source => source.Id == this.ViewModel.SelectedFundingSource?.Id)
                    ?? this.ViewModel.FundingSources[FirstItemIndex];
                this.WithdrawDestComboBox.SelectedItem = defaultFundingSource;
                this.ViewModel.WithdrawDestination = defaultFundingSource;
            }

            this.WithdrawAmountTextBox.Text = string.Empty;
            this.ViewModel.WithdrawAmountText = string.Empty;
            this.WithdrawResultBar.IsOpen = false;

            // Show penalty warning if applicable
            this.WithdrawPenaltyWarning.Visibility = this.ViewModel.WithdrawHasEarlyRisk
                ? Visibility.Visible
                : Visibility.Collapsed;
            this.WithdrawPenaltySummaryText.Text = this.ViewModel.WithdrawPenaltySummary;
            this.WithdrawPenaltyBreakdown.Visibility = Visibility.Collapsed;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.WithdrawActionPanel.Visibility = Visibility.Visible;
        }

        private async void OnAutoDepositClicked(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedAccount == null)
            {
                return;
            }

            await this.ViewModel.LoadAutoDepositAsync(this.ViewModel.SelectedAccount.IdentificationNumber);

            this.AutoDepositTitle.Text = this.ViewModel.ExistingLabel + " Auto Deposit";
            this.AutoDepositAmountTextBox.Text = this.ViewModel.AutoDepositAmountText;

            // Set frequency radio
            this.AutoDepositFrequencyRadios.SelectedIndex = NoSelectionIndex;
            for (var i = FirstItemIndex; i < this.AutoDepositFrequencyRadios.Items.Count; i++)
            {
                if (this.AutoDepositFrequencyRadios.Items[i] is RadioButton radioButton &&
                    radioButton.Tag?.ToString() == this.ViewModel.AutoDepositFrequency)
                {
                    this.AutoDepositFrequencyRadios.SelectedIndex = i;
                    break;
                }
            }

            this.AutoDepositStartDatePicker.Date = this.ViewModel.AutoDepositStartDate;
            this.AutoDepositActiveToggle.IsOn = this.ViewModel.AutoDepositIsActive;
            this.AutoDepositResultBar.IsOpen = false;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.AutoDepositActionPanel.Visibility = Visibility.Visible;
        }

        private async void OnCloseAccountClicked(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedAccount == null)
            {
                return;
            }

            await this.ViewModel.LoadCloseDestinationAccountsAsync();

            this.CloseDestComboBox.ItemsSource = this.ViewModel.CloseDestinationAccounts;
            this.CloseResultBar.IsOpen = false;
            this.CloseConfirmCheckBox.IsChecked = false;
            this.CloseConfirmButton.IsEnabled = false;

            var hasNoDest = !this.ViewModel.CloseDestinationAccounts.Any();
            this.CloseNoDestText.Visibility = hasNoDest ? Visibility.Visible : Visibility.Collapsed;
            this.CloseDestComboBox.Visibility = hasNoDest ? Visibility.Collapsed : Visibility.Visible;

            if (!hasNoDest)
            {
                this.CloseDestComboBox.SelectedIndex = FirstItemIndex;
            }

            // Show penalty warning for fixed deposit before maturity
            this.ClosePenaltyWarning.Visibility = this.ViewModel.CloseHasPenalty
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.CloseAccountActionPanel.Visibility = Visibility.Visible;
        }

        // ── Deposit action ───────────────────────────────────────────────────
        private void OnDepositAmountChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.DepositAmountText = this.DepositAmountTextBox.Text;
            this.DepositLivePreview.Text = this.ViewModel.LivePreview;
        }

        private void OnDepositSourceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DepositSourceComboBox.SelectedItem is FundingSourceOption fundingSourceOption)
            {
                this.ViewModel.DepositSource = fundingSourceOption.DisplayName;
            }
        }

        private async void OnDepositConfirmed(object sender, RoutedEventArgs e)
        {
            this.DepositResultBar.IsOpen = false;
            await this.ViewModel.DepositAsync();

            if (this.ViewModel.HasError)
            {
                this.DepositResultBar.Severity = InfoBarSeverity.Error;
                this.DepositResultBar.Message = this.ViewModel.ErrorMessage;
                this.DepositResultBar.IsOpen = true;
            }
            else if (this.ViewModel.ShowDepositSuccess)
            {
                this.DepositResultBar.Severity = InfoBarSeverity.Success;
                this.DepositResultBar.Message = this.ViewModel.DepositSuccessMessage;
                this.DepositResultBar.IsOpen = true;
                this.DepositAmountTextBox.Text = string.Empty;
            }
        }

        private void OnDepositBack(object sender, RoutedEventArgs e)
        {
            this.DepositActionPanel.Visibility = Visibility.Collapsed;
            this.ManageButtonsPanel.Visibility = Visibility.Visible;
        }

        // ── Withdraw action ──────────────────────────────────────────────────
        private void OnWithdrawAmountChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.WithdrawAmountText = this.WithdrawAmountTextBox.Text;

            var hasPenalty = this.ViewModel.WithdrawHasPenalty;
            this.WithdrawPenaltyBreakdown.Visibility = hasPenalty ? Visibility.Visible : Visibility.Collapsed;
            if (hasPenalty)
            {
                this.WithdrawPenaltyAmountText.Text = this.ViewModel.WithdrawPenaltyBreakdownText;
                this.WithdrawNetAmountText.Text = this.ViewModel.WithdrawNetAmountText;
            }
        }

        private void OnWithdrawDestChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.WithdrawDestination = this.WithdrawDestComboBox.SelectedItem as FundingSourceOption;
        }

        private async void OnWithdrawConfirmed(object sender, RoutedEventArgs e)
        {
            this.WithdrawResultBar.IsOpen = false;
            var success = await this.ViewModel.ConfirmWithdrawAsync();

            this.WithdrawResultBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            this.WithdrawResultBar.Message = this.ViewModel.WithdrawResultMessage;
            this.WithdrawResultBar.IsOpen = true;

            if (success)
            {
                this.WithdrawAmountTextBox.Text = string.Empty;
            }
        }

        private void OnWithdrawBack(object sender, RoutedEventArgs e)
        {
            this.WithdrawActionPanel.Visibility = Visibility.Collapsed;
            this.ManageButtonsPanel.Visibility = Visibility.Visible;
        }

        // ── Auto Deposit action ──────────────────────────────────────────────
        private void OnAutoDepositFrequencyChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AutoDepositFrequencyRadios.SelectedItem is RadioButton radioButton)
            {
                this.ViewModel.AutoDepositFrequency = radioButton.Tag?.ToString() ?? string.Empty;
            }
        }

        private async void OnAutoDepositSaved(object sender, RoutedEventArgs e)
        {
            this.AutoDepositResultBar.IsOpen = false;

            this.ViewModel.AutoDepositAmountText = this.AutoDepositAmountTextBox.Text;
            this.ViewModel.AutoDepositStartDate = this.AutoDepositStartDatePicker.Date;
            this.ViewModel.AutoDepositIsActive = this.AutoDepositActiveToggle.IsOn;

            await this.ViewModel.SaveAutoDepositAsync();

            if (this.ViewModel.HasError)
            {
                this.AutoDepositResultBar.Severity = InfoBarSeverity.Error;
                this.AutoDepositResultBar.Message = this.ViewModel.ErrorMessage;
                this.AutoDepositResultBar.IsOpen = true;
            }
            else if (!string.IsNullOrEmpty(this.ViewModel.AutoDepositSaveMessage))
            {
                this.AutoDepositResultBar.Severity = InfoBarSeverity.Success;
                this.AutoDepositResultBar.Message = this.ViewModel.AutoDepositSaveMessage;
                this.AutoDepositResultBar.IsOpen = true;
                this.AutoDepositTitle.Text = "Modify Auto Deposit";
            }
        }

        private void OnAutoDepositBack(object sender, RoutedEventArgs e)
        {
            this.AutoDepositActionPanel.Visibility = Visibility.Collapsed;
            this.ManageButtonsPanel.Visibility = Visibility.Visible;
        }

        // ── Close Account action ─────────────────────────────────────────────
        private void OnCloseDestChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CloseDestComboBox.SelectedItem is SavingsAccount savingsAccount)
            {
                this.ViewModel.SelectedCloseDestinationId = savingsAccount.IdentificationNumber;
            }
        }

        private void OnCloseConfirmChecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CloseUserConfirmed = true;
            this.CloseConfirmButton.IsEnabled = true;
        }

        private void OnCloseConfirmUnchecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CloseUserConfirmed = false;
            this.CloseConfirmButton.IsEnabled = false;
        }

        private async void OnCloseConfirmed(object sender, RoutedEventArgs e)
        {
            this.CloseResultBar.IsOpen = false;
            var success = await this.ViewModel.ConfirmCloseAsync();

            this.CloseResultBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            this.CloseResultBar.Message = this.ViewModel.CloseResultMessage;
            this.CloseResultBar.IsOpen = true;

            if (success)
            {
                // After successful close, go back to buttons panel after a brief moment
                await Task.Delay(SuccessMessageDelayMilliseconds);
                this.CloseAccountActionPanel.Visibility = Visibility.Collapsed;
                this.ManageButtonsPanel.Visibility = Visibility.Visible;
                this.ManageAccountComboBox.SelectedIndex = NoSelectionIndex;
                this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void OnCloseBack(object sender, RoutedEventArgs e)
        {
            this.CloseAccountActionPanel.Visibility = Visibility.Collapsed;
            this.ManageButtonsPanel.Visibility = Visibility.Visible;
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private void HideAllActionPanels()
        {
            this.DepositActionPanel.Visibility = Visibility.Collapsed;
            this.WithdrawActionPanel.Visibility = Visibility.Collapsed;
            this.AutoDepositActionPanel.Visibility = Visibility.Collapsed;
            this.CloseAccountActionPanel.Visibility = Visibility.Collapsed;
        }

        private void ClearCreateErrors()
        {
            this.CreateErrorBar.IsOpen = false;
            this.CreateSuccessBar.IsOpen = false;
            this.TypeErrorText.Visibility = Visibility.Collapsed;
            this.AccountNameError.Visibility = Visibility.Collapsed;
            this.InitialDepositError.Visibility = Visibility.Collapsed;
            this.FundingSourceError.Visibility = Visibility.Collapsed;
            this.FrequencyError.Visibility = Visibility.Collapsed;
            this.TargetAmountError.Visibility = Visibility.Collapsed;
            this.TargetDateError.Visibility = Visibility.Collapsed;
        }

        private string GetSelectedRadioButtonTag(RadioButtons radioButtons)
        {
            return (radioButtons.SelectedItem as RadioButton)?.Tag?.ToString() ?? string.Empty;
        }

        private void ResetCreateFormControls()
        {
            this.AccountNameTextBox.Text = string.Empty;
            this.InitialDepositTextBox.Text = string.Empty;
            this.TargetAmountTextBox.Text = string.Empty;
            this.TargetDatePicker.Date = null;
            this.MaturityDatePicker.Date = null;
            this.SavingsTypeRadioButtons.SelectedIndex = NoSelectionIndex;
            if (this.ViewModel != null)
            {
                this.ViewModel.SelectedSavingsType = string.Empty;
            }
            this.SetDefaultCreateFrequency();
            this.GoalSavingsPanel.Visibility = Visibility.Collapsed;
            this.FixedDepositPanel.Visibility = Visibility.Collapsed;
            this.ClearCreateErrors();
        }

        private void SetDefaultCreateFrequency()
        {
            if (this.ViewModel == null)
            {
                return;
            }

            this.FrequencyRadioButtons.SelectedIndex = FirstItemIndex;
            this.ViewModel.SelectedFrequency = OneTimeFrequencyTag;
        }

        private static void ShowError(TextBlock tb, string msg)
        {
            tb.Text = msg;
            tb.Visibility = Visibility.Visible;
        }

        private async Task ShowDialogAsync(string title, string msg)
        {
            var contentDialog = new ContentDialog
            {
                Title = title,
                Content = msg,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await contentDialog.ShowAsync();
        }
    }
}
