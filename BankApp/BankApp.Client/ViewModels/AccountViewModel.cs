using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.Entities;
using Microsoft.UI.Xaml;

namespace BankApp.Client.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly IAccountService _accountService;
        private readonly IAuthService _authService;
        private readonly AsyncRelayCommand _refreshCommand;

        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _hasError;
        private Account? _selectedAccount;

        public AccountViewModel(IAccountService accountService, IAuthService authService)
        {
            _accountService = accountService;
            _authService = authService;
            Accounts = new ObservableCollection<Account>();
            _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !_isLoading);
        }

        public ObservableCollection<Account> Accounts { get; }

        public AsyncRelayCommand RefreshCommand => _refreshCommand;

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    _refreshCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(LoadingVisibility));
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            private set => SetProperty(ref _hasError, value);
        }

        public Account? SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        public int AccountCount => Accounts.Count;

        public decimal TotalBalance => Accounts.Sum(account => account.Balance);

        public async Task LoadAsync()
        {
            if (!_authService.IsAuthenticated())
            {
                StatusMessage = "You must sign in to view accounts.";
                HasError = true;
                return;
            }

            try
            {
                IsLoading = true;
                HasError = false;
                StatusMessage = string.Empty;

                var accounts = await _accountService.GetAccountsAsync();
                Accounts.Clear();

                foreach (Account account in accounts)
                {
                    Accounts.Add(account);
                }

                OnPropertyChanged(nameof(AccountCount));
                OnPropertyChanged(nameof(TotalBalance));

                if (Accounts.Count == 0)
                {
                    StatusMessage = "No accounts were found for the current session.";
                    HasError = false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Your session expired. Please sign in again.";
                HasError = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unable to load accounts: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(AccountCount));
                OnPropertyChanged(nameof(TotalBalance));
            }
        }

        public override void Dispose()
        {
        }
    }
}