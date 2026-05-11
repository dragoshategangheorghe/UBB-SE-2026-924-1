namespace BankApp.Client.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using BankApp.Models.Entities;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    /// ViewModel for managing bank accounts.
    /// </summary>
    public partial class AccountViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Account> accounts = new ();

        [ObservableProperty]
        private bool isBusy;

        /// <summary>
        /// Loads accounts for a specific user via the AccountService.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A task representing the operation.</returns>
        public async Task LoadAccountsAsync(int userId)
        {
            this.IsBusy = true;
            try
            {
                // Refactored to use the Service instead of direct ApiService
                var result = await App.AccountService.GetUserAccountsAsync(userId);

                if (result != null)
                {
                    this.Accounts.Clear();
                    foreach (var account in result)
                    {
                        this.Accounts.Add(account);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading accounts: {ex.Message}");
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}