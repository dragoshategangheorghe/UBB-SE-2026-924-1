namespace BankApp.Client.Views
{
    using System;
    using System.Diagnostics;
    using BankApp.Client.View.Dialogs;
    using BankApp.Client.ViewModels;
    using BankApp.Client.Views.Dialogs;
    using BankApp.Models.Entities;
    using BankApp.Models.Enums;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class LoansView : UserControl
    {
        public LoansViewModel? ViewModel => this.DataContext as LoansViewModel;

        public LoansView()
        {
            this.InitializeComponent();
            this.Loaded += LoansView_Loaded;
        }

        private async void LoansView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                try
                {
                    var userId = App.AuthService.GetCurrentUserId() ?? throw new Exception("Current user id is null.");
                    this.ViewModel.CurrentUser = new User { Id = userId };

                    await this.ViewModel.LoadLoansAsync();
                }
                catch (Exception ex)
                {
                    this.ViewModel.ErrorMessage = ex.Message;
                }
            }
        }

        private async void OnApplyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new LoanApplicationDialog(this.ViewModel)
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
                    this.ViewModel.SelectedLoan = loan;
                    var dialog = new PayInstallmentDialog(this.ViewModel)
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
                    this.ViewModel.SelectedLoan = loan;
                    await this.ViewModel.LoadAmortizationAsync();

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
            this.ViewModel.StatusFilter = null;
        }

        private void OnFilterActive(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StatusFilter = LoanStatus.Active;
        }

        private void OnFilterClosed(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StatusFilter = LoanStatus.Passed;
        }

        private void OnTypeFilterAll(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TypeFilter = null;
        }

        private void OnTypeFilterPersonal(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TypeFilter = LoanType.Personal;
        }

        private void OnTypeFilterMortgage(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TypeFilter = LoanType.Mortgage;
        }

        private void OnTypeFilterStudent(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TypeFilter = LoanType.Student;
        }

        private void OnTypeFilterAuto(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TypeFilter = LoanType.Auto;
        }
    }
}