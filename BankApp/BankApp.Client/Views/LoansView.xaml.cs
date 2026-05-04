namespace BankApp.Client.Views
{
    using System;
    using System.Diagnostics;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using BankApp.Client.ViewModels;
    using BankApp.Client.Views.Dialogs;
    using BankApp.Models.Enums;

    public sealed partial class LoansView : Page
    {
        private readonly LoansViewModel viewModel;

        public LoansView()
        {
            this.InitializeComponent();
            this.viewModel = new LoansViewModel();
            this.DataContext = this.viewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await this.viewModel.LoadLoansAsync();
        }

        private async void OnApplyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new LoanApplicationDialog(this.viewModel)
                {
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void OnPayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is LoanViewModel loan)
                {
                    this.viewModel.SelectedLoan = loan;
                    var dialog = new PayInstallmentDialog(this.viewModel)
                    {
                        XamlRoot = this.XamlRoot,
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void OnScheduleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is LoanViewModel loan)
                {
                    this.viewModel.SelectedLoan = loan;
                    await this.viewModel.LoadAmortizationAsync();
                    this.Frame.Navigate(typeof(AmortizationScheduleView), loan.Loan);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void OnFilterAll(object sender, RoutedEventArgs e)
        {
            this.viewModel.StatusFilter = null;
        }

        private void OnFilterActive(object sender, RoutedEventArgs e)
        {
            this.viewModel.StatusFilter = LoanStatus.Active;
        }

        private void OnFilterClosed(object sender, RoutedEventArgs e)
        {
            this.viewModel.StatusFilter = LoanStatus.Passed;
        }

        private void OnTypeFilterAll(object sender, RoutedEventArgs e)
        {
            this.viewModel.TypeFilter = null;
        }

        private void OnTypeFilterPersonal(object sender, RoutedEventArgs e)
        {
            this.viewModel.TypeFilter = LoanType.Personal;
        }

        private void OnTypeFilterMortgage(object sender, RoutedEventArgs e)
        {
            this.viewModel.TypeFilter = LoanType.Mortgage;
        }

        private void OnTypeFilterStudent(object sender, RoutedEventArgs e)
        {
            this.viewModel.TypeFilter = LoanType.Student;
        }

        private void OnTypeFilterAuto(object sender, RoutedEventArgs e)
        {
            this.viewModel.TypeFilter = LoanType.Auto;
        }
    }
}