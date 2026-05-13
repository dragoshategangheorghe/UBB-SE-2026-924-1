namespace BankApp.Client.View.Dialogs
{
    using System;
    using System.Diagnostics;
    using BankApp.Client.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class PayInstallmentDialog : ContentDialog
    {
        private readonly LoansViewModel viewModel;

        public PayInstallmentDialog(LoansViewModel viewModel)
        {
            this.InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
            this.UpdatePreview();
        }

        private async void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                Debug.WriteLine($"CustomAmount before pay: {this.viewModel.CustomAmount}");
                Debug.WriteLine($"SelectedLoan: {this.viewModel.SelectedLoan?.Loan?.Id}");
                await this.viewModel.PayInstallmentAsync();
                Debug.WriteLine("PayInstallmentAsync completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PAY ERROR: {ex.Message}");
                Debug.WriteLine($"PAY INNER: {ex.InnerException?.Message}");
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnStandardChecked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            if (this.CustomAmountPanel != null)
            {
                this.CustomAmountPanel.Visibility = Visibility.Collapsed;
            }

            this.viewModel.SelectStandardPayment();
            this.UpdatePreview();
        }

        private void OnCustomChecked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.CustomAmountPanel.Visibility = Visibility.Visible;
            if (this.viewModel.SelectedLoan != null)
            {
                this.CustomAmountBox.Text = this.viewModel.SelectCustomPayment();
            }

            this.UpdatePreview();
        }

        private void OnCustomAmountTextChanged(object sender, TextChangedEventArgs e)
        {
            this.viewModel.UpdateCustomPayment(this.CustomAmountBox?.Text ?? string.Empty);
            this.UpdatePreview();
        }

        private void OnCustomAmountLostFocus(object sender, RoutedEventArgs e)
        {
            this.viewModel.UpdateCustomPayment(this.CustomAmountBox?.Text ?? string.Empty);
            this.UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (this.viewModel == null)
            {
                return;
            }

            if (this.viewModel.SelectedLoan == null)
            {
                this.BalanceAfterPaymentText.Text = string.Empty;
                this.RemainingTermAfterPaymentText.Text = string.Empty;
                return;
            }

            if (this.StandardRadio.IsChecked == true)
            {
                this.viewModel.SelectStandardPayment();
            }
            else
            {
                this.viewModel.UpdateCustomPayment(this.CustomAmountBox?.Text ?? string.Empty);
            }

            this.BalanceAfterPaymentText.Text = this.viewModel.PaymentPreviewBalance.ToString("C2");
            this.RemainingTermAfterPaymentText.Text = $"{this.viewModel.PaymentPreviewRemainingMonths} mo";
        }
    }
}