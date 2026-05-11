using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BankApp.Client.ViewModels
{
    public partial class LoansAndSavingsViewModel : BaseViewModel
    {
        public SavingsViewModel SavingsVM { get; }

        public LoansViewModel LoansVM { get; }

        public LoansAndSavingsViewModel(SavingsViewModel savingsVM, LoansViewModel loansVM)
        {
            SavingsVM = savingsVM;
            LoansVM = loansVM;
        }

        public override void Dispose()
        {
            SavingsVM.Dispose();
        }
    }
}