namespace BankApp.Client.Views
{
    using BankApp.Client.ViewModels;
    using BankApp.Models.Features.Investments;
    using BankApp.Models.Features.Savings;
    using BankApp.Server.Repositories.Implementations;
    using BankApp.Server.Services.Implementations;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed partial class SavingsView : UserControl
    {
        private const int FirstItemIndex = 0;
        private const int NoSelectionIndex = -1;
        private const int SuccessMessageDelayMilliseconds = 1500;

        private readonly SavingsViewModel viewModel;

        public SavingsView(SavingsViewModel savingsViewModel)
        {
            this.InitializeComponent();
            this.viewModel = savingsViewModel;
            this.DataContext = this.viewModel;
            this.MainNavigationView.SelectedItem = this.MyAccountsTab;
            this.Loaded += SavingsView_Loaded;
        }

        private async void SavingsView_Loaded(object sender, RoutedEventArgs e)
        {
            await this.viewModel.LoadAccountsAsync();

            if (this.viewModel.HasError)
            {
                await this.ShowDialogAsync("Load Error", this.viewModel.ErrorMessage);
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
                this.SavingsTypeRadioButtons.SelectedIndex = NoSelectionIndex;
                this.FrequencyRadioButtons.SelectedIndex = NoSelectionIndex;
                this.viewModel.SelectedSavingsType = string.Empty;
                this.viewModel.SelectedFrequency = string.Empty;
                this.GoalSavingsPanel.Visibility = Visibility.Collapsed;
                this.ClearCreateErrors();

                await this.viewModel.LoadFundingSourcesAsync();
                this.FundingSourceComboBox.ItemsSource = this.viewModel.FundingSources;
                if (this.viewModel.FundingSources.Any())
                {
                    this.FundingSourceComboBox.SelectedIndex = FirstItemIndex;
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
                this.viewModel.SelectedFrequency = radioButton.Tag?.ToString() ?? string.Empty;
            }
        }

        private void OnSavingsTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SavingsTypeRadioButtons.SelectedItem is RadioButton radioButton)
            {
                this.viewModel.SelectedSavingsType = radioButton.Tag?.ToString() ?? string.Empty;

                this.GoalSavingsPanel.Visibility = this.viewModel.SelectedSavingsType == "GoalSavings"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                this.FixedDepositPanel.Visibility = this.viewModel.SelectedSavingsType == "FixedDeposit"
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private async void OnOpenAccountClicked(object sender, RoutedEventArgs e)
        {
            this.ClearCreateErrors();
            this.viewModel.PrepareCreateAccountSubmission(
                this.AccountNameTextBox.Text,
                this.InitialDepositTextBox.Text,
                this.FundingSourceComboBox.SelectedItem as FundingSourceOption,
                this.TargetAmountTextBox.Text,
                this.TargetDatePicker.Date,
                this.MaturityDatePicker.Date);

            await this.viewModel.CreateAccountCommand.ExecuteAsync(null);

            if (this.viewModel.FieldErrors.TryGetValue("SavingsType", out var savingsTypeError))
            {
                ShowError(this.TypeErrorText, savingsTypeError);
            }

            if (this.viewModel.FieldErrors.TryGetValue("AccountName", out var accountNameError))
            {
                ShowError(this.AccountNameError, accountNameError);
            }

            if (this.viewModel.FieldErrors.TryGetValue("InitialDeposit", out var initialDepositError))
            {
                ShowError(this.InitialDepositError, initialDepositError);
            }

            if (this.viewModel.FieldErrors.TryGetValue("FundingSource", out var fundingSourceError))
            {
                ShowError(this.FundingSourceError, fundingSourceError);
            }

            if (this.viewModel.FieldErrors.TryGetValue("Frequency", out var frequencyError))
            {
                ShowError(this.FrequencyError, frequencyError);
            }

            if (this.viewModel.FieldErrors.TryGetValue("TargetAmount", out var targetAmountError))
            {
                ShowError(this.TargetAmountError, targetAmountError);
            }

            if (this.viewModel.FieldErrors.TryGetValue("TargetDate", out var targetDateError))
            {
                ShowError(this.TargetDateError, targetDateError);
            }

            if (this.viewModel.HasError)
            {
                this.CreateErrorBar.Message = this.viewModel.ErrorMessage;
                this.CreateErrorBar.IsOpen = true;
                return;
            }

            if (this.viewModel.ShowCreateConfirmation)
            {
                this.CreateSuccessBar.IsOpen = true;
                this.OpenAccountButton.IsEnabled = false;
                await Task.Delay(SuccessMessageDelayMilliseconds);
                this.CreateSuccessBar.IsOpen = false;
                this.OpenAccountButton.IsEnabled = true;
                this.AccountNameTextBox.Text = string.Empty;
                this.InitialDepositTextBox.Text = string.Empty;
                this.SavingsTypeRadioButtons.SelectedIndex = NoSelectionIndex;
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
            this.viewModel.SelectedAccount = this.ManageAccountComboBox.SelectedItem as SavingsAccount;
            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = this.viewModel.SelectedAccount != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ── Manage: show action panels ───────────────────────────────────────
        private async void OnDepositClicked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.SelectedAccount == null)
            {
                return;
            }

            // Load funding sources into the deposit combobox
            await this.viewModel.LoadFundingSourcesAsync();
            this.DepositSourceComboBox.ItemsSource = this.viewModel.FundingSources;
            if (this.viewModel.FundingSources.Any())
            {
                this.DepositSourceComboBox.SelectedIndex = FirstItemIndex;
            }

            // Sync amount field
            this.DepositAmountTextBox.Text = string.Empty;
            this.viewModel.DepositAmountText = string.Empty;
            this.DepositLivePreview.Text = string.Empty;
            this.DepositResultBar.IsOpen = false;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.DepositActionPanel.Visibility = Visibility.Visible;
        }

        private async void OnWithdrawClicked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.SelectedAccount == null)
            {
                return;
            }

            // Load funding sources as withdraw destinations
            await this.viewModel.LoadFundingSourcesAsync();
            this.WithdrawDestComboBox.ItemsSource = this.viewModel.FundingSources;
            if (this.viewModel.FundingSources.Any())
            {
                this.WithdrawDestComboBox.SelectedIndex = FirstItemIndex;
                this.viewModel.WithdrawDestination = this.viewModel.FundingSources[FirstItemIndex];
            }

            this.WithdrawAmountTextBox.Text = string.Empty;
            this.viewModel.WithdrawAmountText = string.Empty;
            this.WithdrawResultBar.IsOpen = false;

            // Show penalty warning if applicable
            this.WithdrawPenaltyWarning.Visibility = this.viewModel.WithdrawHasEarlyRisk
                ? Visibility.Visible
                : Visibility.Collapsed;
            this.WithdrawPenaltySummaryText.Text = this.viewModel.WithdrawPenaltySummary;
            this.WithdrawPenaltyBreakdown.Visibility = Visibility.Collapsed;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.WithdrawActionPanel.Visibility = Visibility.Visible;
        }

        private async void OnAutoDepositClicked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.SelectedAccount == null)
            {
                return;
            }

            await this.viewModel.LoadAutoDepositAsync(this.viewModel.SelectedAccount.IdentificationNumber);

            this.AutoDepositTitle.Text = this.viewModel.ExistingLabel + " Auto Deposit";
            this.AutoDepositAmountTextBox.Text = this.viewModel.AutoDepositAmountText;

            // Set frequency radio
            this.AutoDepositFrequencyRadios.SelectedIndex = NoSelectionIndex;
            for (var i = FirstItemIndex; i < this.AutoDepositFrequencyRadios.Items.Count; i++)
            {
                if (this.AutoDepositFrequencyRadios.Items[i] is RadioButton radioButton &&
                    radioButton.Tag?.ToString() == this.viewModel.AutoDepositFrequency)
                {
                    this.AutoDepositFrequencyRadios.SelectedIndex = i;
                    break;
                }
            }

            this.AutoDepositStartDatePicker.Date = this.viewModel.AutoDepositStartDate;
            this.AutoDepositActiveToggle.IsOn = this.viewModel.AutoDepositIsActive;
            this.AutoDepositResultBar.IsOpen = false;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.AutoDepositActionPanel.Visibility = Visibility.Visible;
        }

        private async void OnCloseAccountClicked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.SelectedAccount == null)
            {
                return;
            }

            await this.viewModel.LoadCloseDestinationAccountsAsync();

            this.CloseDestComboBox.ItemsSource = this.viewModel.CloseDestinationAccounts;
            this.CloseResultBar.IsOpen = false;
            this.CloseConfirmCheckBox.IsChecked = false;
            this.CloseConfirmButton.IsEnabled = false;

            var hasNoDest = !this.viewModel.CloseDestinationAccounts.Any();
            this.CloseNoDestText.Visibility = hasNoDest ? Visibility.Visible : Visibility.Collapsed;
            this.CloseDestComboBox.Visibility = hasNoDest ? Visibility.Collapsed : Visibility.Visible;

            if (!hasNoDest)
            {
                this.CloseDestComboBox.SelectedIndex = FirstItemIndex;
            }

            // Show penalty warning for fixed deposit before maturity
            this.ClosePenaltyWarning.Visibility = this.viewModel.CloseHasPenalty
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.HideAllActionPanels();
            this.ManageButtonsPanel.Visibility = Visibility.Collapsed;
            this.CloseAccountActionPanel.Visibility = Visibility.Visible;
        }

        // ── Deposit action ───────────────────────────────────────────────────
        private void OnDepositAmountChanged(object sender, TextChangedEventArgs e)
        {
            this.viewModel.DepositAmountText = this.DepositAmountTextBox.Text;
            this.DepositLivePreview.Text = this.viewModel.LivePreview;
        }

        private void OnDepositSourceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DepositSourceComboBox.SelectedItem is FundingSourceOption fundingSourceOption)
            {
                this.viewModel.DepositSource = fundingSourceOption.DisplayName;
            }
        }

        private async void OnDepositConfirmed(object sender, RoutedEventArgs e)
        {
            this.DepositResultBar.IsOpen = false;
            await this.viewModel.DepositAsync();

            if (this.viewModel.HasError)
            {
                this.DepositResultBar.Severity = InfoBarSeverity.Error;
                this.DepositResultBar.Message = this.viewModel.ErrorMessage;
                this.DepositResultBar.IsOpen = true;
            }
            else if (this.viewModel.ShowDepositSuccess)
            {
                this.DepositResultBar.Severity = InfoBarSeverity.Success;
                this.DepositResultBar.Message = this.viewModel.DepositSuccessMessage;
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
            this.viewModel.WithdrawAmountText = this.WithdrawAmountTextBox.Text;

            var hasPenalty = this.viewModel.WithdrawHasPenalty;
            this.WithdrawPenaltyBreakdown.Visibility = hasPenalty ? Visibility.Visible : Visibility.Collapsed;
            if (hasPenalty)
            {
                this.WithdrawPenaltyAmountText.Text = this.viewModel.WithdrawPenaltyBreakdownText;
                this.WithdrawNetAmountText.Text = this.viewModel.WithdrawNetAmountText;
            }
        }

        private void OnWithdrawDestChanged(object sender, SelectionChangedEventArgs e)
        {
            this.viewModel.WithdrawDestination = this.WithdrawDestComboBox.SelectedItem as FundingSourceOption;
        }

        private async void OnWithdrawConfirmed(object sender, RoutedEventArgs e)
        {
            this.WithdrawResultBar.IsOpen = false;
            var success = await this.viewModel.ConfirmWithdrawAsync();

            this.WithdrawResultBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            this.WithdrawResultBar.Message = this.viewModel.WithdrawResultMessage;
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
                this.viewModel.AutoDepositFrequency = radioButton.Tag?.ToString() ?? string.Empty;
            }
        }

        private async void OnAutoDepositSaved(object sender, RoutedEventArgs e)
        {
            this.AutoDepositResultBar.IsOpen = false;

            this.viewModel.AutoDepositAmountText = this.AutoDepositAmountTextBox.Text;
            this.viewModel.AutoDepositStartDate = this.AutoDepositStartDatePicker.Date;
            this.viewModel.AutoDepositIsActive = this.AutoDepositActiveToggle.IsOn;

            await this.viewModel.SaveAutoDepositAsync();

            if (this.viewModel.HasError)
            {
                this.AutoDepositResultBar.Severity = InfoBarSeverity.Error;
                this.AutoDepositResultBar.Message = this.viewModel.ErrorMessage;
                this.AutoDepositResultBar.IsOpen = true;
            }
            else if (!string.IsNullOrEmpty(this.viewModel.AutoDepositSaveMessage))
            {
                this.AutoDepositResultBar.Severity = InfoBarSeverity.Success;
                this.AutoDepositResultBar.Message = this.viewModel.AutoDepositSaveMessage;
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
                this.viewModel.SelectedCloseDestinationId = savingsAccount.IdentificationNumber;
            }
        }

        private void OnCloseConfirmChecked(object sender, RoutedEventArgs e)
        {
            this.viewModel.CloseUserConfirmed = true;
            this.CloseConfirmButton.IsEnabled = true;
        }

        private void OnCloseConfirmUnchecked(object sender, RoutedEventArgs e)
        {
            this.viewModel.CloseUserConfirmed = false;
            this.CloseConfirmButton.IsEnabled = false;
        }

        private async void OnCloseConfirmed(object sender, RoutedEventArgs e)
        {
            this.CloseResultBar.IsOpen = false;
            var success = await this.viewModel.ConfirmCloseAsync();

            this.CloseResultBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            this.CloseResultBar.Message = this.viewModel.CloseResultMessage;
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
            this.TargetAmountError.Visibility = Visibility.Collapsed;
            this.TargetDateError.Visibility = Visibility.Collapsed;
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