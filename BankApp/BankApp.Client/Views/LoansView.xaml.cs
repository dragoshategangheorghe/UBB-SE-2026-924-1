namespace BankApp.Client.Views
{
    using System;
    using System.Diagnostics;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Microsoft.UI.Xaml.Media;
    using BankApp.Client.ViewModels;
    using BankApp.Client.Views.Dialogs;
    using BankApp.Models.Enums;
    using BankApp.Client.View.Dialogs;

    public sealed partial class LoansView : UserControl
    {
        public LoansViewModel? viewModel => this.DataContext as LoansViewModel;

        public LoansView()
        {
            this.InitializeComponent();
            this.Loaded += LoansView_Loaded;
        }

        private async void LoansView_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                await this.viewModel.LoadLoansAsync();
            }
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

                    Frame? mainFrame = GetParentFrame();

                    if (mainFrame != null)
                    {
                        mainFrame.Navigate(typeof(AmortizationScheduleView), loan.Loan);
                    }
                    else
                    {
                        Debug.WriteLine("Nu s-a putut gasi un Frame pentru navigare.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private Frame? GetParentFrame()
        {
            DependencyObject current = this;

            while (current != null)
            {
                if (current is Frame frame)
                {
                    return frame;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
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