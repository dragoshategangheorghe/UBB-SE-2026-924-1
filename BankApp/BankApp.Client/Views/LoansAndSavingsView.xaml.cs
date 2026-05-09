using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using BankApp.Client.ViewModels;
namespace BankApp.Client.Views
{
    public sealed partial class LoansAndSavingsView : Page
    {
        public LoansAndSavingsView()
        {
            this.InitializeComponent();
            ViewModel = new LoansAndSavingsViewModel(
                new SavingsViewModel(App.SavingsService),
                new LoansViewModel(App.LoansService));
        }

        public LoansAndSavingsViewModel ViewModel
        {
            get => (LoansAndSavingsViewModel)this.DataContext;
            set => this.DataContext = value;
        }
    }
}