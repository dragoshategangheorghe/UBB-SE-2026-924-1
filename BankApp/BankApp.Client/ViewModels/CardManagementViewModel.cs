using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Cards;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.ViewModels
{
    public class CardManagementViewModel : BaseViewModel
    {
        private readonly ICardApiService _cardApiService;
        private readonly AsyncRelayCommand _refreshCommand;
        private readonly AsyncRelayCommand _applySortCommand;
        private readonly AsyncRelayCommand _freezeCommand;
        private readonly AsyncRelayCommand _unfreezeCommand;
        private readonly AsyncRelayCommand _saveSettingsCommand;
        private readonly RelayCommand _hideSensitiveDetailsCommand;

        private bool _isLoading;
        private bool _isStatusOpen;
        private InfoBarSeverity _statusSeverity;
        private string _statusMessage = string.Empty;
        private CardSummaryDto? _selectedCard;
        private string _selectedSortOption = CardSortOptions.Custom;
        private string _spendingLimitInput = string.Empty;
        private bool _isSensitiveDetailsVisible;
        private string _revealedCardNumber = string.Empty;
        private string _revealedCvv = string.Empty;
        private string _revealCountdownText = string.Empty;
        private CancellationTokenSource? _autoHideCancellation;

        public CardManagementViewModel(ICardApiService cardApiService)
        {
            _cardApiService = cardApiService;

            Cards = new ObservableCollection<CardSummaryDto>();
            SortOptions = new List<SelectableOption>
            {
                new SelectableOption(CardSortOptions.Custom, "Custom"),
                new SelectableOption(CardSortOptions.CardholderName, "Cardholder Name"),
                new SelectableOption(CardSortOptions.ExpiryDate, "Expiry Date"),
                new SelectableOption(CardSortOptions.Status, "Status")
            };

            _refreshCommand = new AsyncRelayCommand(LoadAsync);
            _applySortCommand = new AsyncRelayCommand(ApplySortPreferenceAsync);
            _freezeCommand = new AsyncRelayCommand(FreezeSelectedCardAsync, () => SelectedCard != null);
            _unfreezeCommand = new AsyncRelayCommand(UnfreezeSelectedCardAsync, () => SelectedCard != null);
            _saveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync, () => SelectedCard != null);
            _hideSensitiveDetailsCommand = new RelayCommand(HideSensitiveDetails, () => IsSensitiveDetailsVisible);
        }

        public ObservableCollection<CardSummaryDto> Cards { get; }

        public IReadOnlyList<SelectableOption> SortOptions { get; }

        public AsyncRelayCommand RefreshCommand => _refreshCommand;

        public AsyncRelayCommand ApplySortCommand => _applySortCommand;

        public AsyncRelayCommand FreezeCommand => _freezeCommand;

        public AsyncRelayCommand UnfreezeCommand => _unfreezeCommand;

        public AsyncRelayCommand SaveSettingsCommand => _saveSettingsCommand;

        public RelayCommand HideSensitiveDetailsCommand => _hideSensitiveDetailsCommand;

        public CardSummaryDto? SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (SetProperty(ref _selectedCard, value))
                {
                    HideSensitiveDetails();
                    SpendingLimitInput = value?.SpendingLimit?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
                    RaiseCardActionStateChanged();
                }
            }
        }

        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set => SetProperty(ref _selectedSortOption, value);
        }

        public string SpendingLimitInput
        {
            get => _spendingLimitInput;
            set => SetProperty(ref _spendingLimitInput, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public bool IsStatusOpen
        {
            get => _isStatusOpen;
            set => SetProperty(ref _isStatusOpen, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public InfoBarSeverity StatusSeverity
        {
            get => _statusSeverity;
            set => SetProperty(ref _statusSeverity, value);
        }

        public bool IsSensitiveDetailsVisible
        {
            get => _isSensitiveDetailsVisible;
            set
            {
                if (SetProperty(ref _isSensitiveDetailsVisible, value))
                {
                    OnPropertyChanged(nameof(SensitiveDetailsVisibility));
                    _hideSensitiveDetailsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public Visibility SensitiveDetailsVisibility => IsSensitiveDetailsVisible ? Visibility.Visible : Visibility.Collapsed;

        public string RevealedCardNumber
        {
            get => _revealedCardNumber;
            set => SetProperty(ref _revealedCardNumber, value);
        }

        public string RevealedCvv
        {
            get => _revealedCvv;
            set => SetProperty(ref _revealedCvv, value);
        }

        public string RevealCountdownText
        {
            get => _revealCountdownText;
            set => SetProperty(ref _revealCountdownText, value);
        }

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                GetCardsResponse? response = await _cardApiService.GetCardsAsync();
                if (response == null || !response.Success)
                {
                    ShowStatus("Failed to load cards.", InfoBarSeverity.Error);
                    return;
                }

                int? selectedCardId = SelectedCard?.Id;
                Cards.Clear();
                foreach (CardSummaryDto card in response.Cards)
                {
                    Cards.Add(card);
                }

                SelectedSortOption = response.SortOption;
                SelectedCard = Cards.FirstOrDefault(card => card.Id == selectedCardId) ?? Cards.FirstOrDefault();

                if (Cards.Count == 0)
                {
                    ShowStatus("No cards are available for this account.", InfoBarSeverity.Informational);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load cards: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<RevealCardResponse?> RevealSensitiveDetailsAsync(string password, string? otpCode)
        {
            if (SelectedCard == null)
            {
                ShowStatus("Select a card first.", InfoBarSeverity.Warning);
                return null;
            }

            try
            {
                RevealCardResponse? response = await _cardApiService.RevealCardAsync(SelectedCard.Id, new RevealCardRequest
                {
                    Password = password,
                    OtpCode = otpCode
                });

                if (response == null)
                {
                    ShowStatus("Unable to reveal card details.", InfoBarSeverity.Error);
                    return null;
                }

                if (response.Success && response.SensitiveDetails != null)
                {
                    RevealedCardNumber = response.SensitiveDetails.CardNumber;
                    RevealedCvv = response.SensitiveDetails.Cvv;
                    IsSensitiveDetailsVisible = true;
                    ShowStatus("Sensitive card details are visible for a limited time.", InfoBarSeverity.Success);
                    StartAutoHideCountdown(response.RevealDurationSeconds);
                }
                else if (response.RequiresOtp)
                {
                    ShowStatus(response.Message, InfoBarSeverity.Informational);
                }
                else
                {
                    ShowStatus(response.Message, InfoBarSeverity.Error);
                }

                return response;
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to reveal card details: {ex.Message}", InfoBarSeverity.Error);
                return null;
            }
        }

        private async Task ApplySortPreferenceAsync()
        {
            try
            {
                CardCommandResponse? response = await _cardApiService.UpdateSortPreferenceAsync(new UpdateCardSortPreferenceRequest
                {
                    SortOption = SelectedSortOption
                });

                if (response?.Success != true)
                {
                    ShowStatus(response?.Message ?? "Failed to update card sort preference.", InfoBarSeverity.Error);
                    return;
                }

                await LoadAsync();
                ShowStatus("Card sort preference updated.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to update sort preference: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private async Task FreezeSelectedCardAsync()
        {
            if (SelectedCard == null)
            {
                return;
            }

            await ExecuteCardUpdateAsync(() => _cardApiService.FreezeCardAsync(SelectedCard.Id));
        }

        private async Task UnfreezeSelectedCardAsync()
        {
            if (SelectedCard == null)
            {
                return;
            }

            await ExecuteCardUpdateAsync(() => _cardApiService.UnfreezeCardAsync(SelectedCard.Id));
        }

        private async Task SaveSettingsAsync()
        {
            if (SelectedCard == null)
            {
                return;
            }

            decimal? spendingLimit = null;
            if (!string.IsNullOrWhiteSpace(SpendingLimitInput))
            {
                if (!decimal.TryParse(SpendingLimitInput, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedLimit))
                {
                    ShowStatus("Enter a valid spending limit.", InfoBarSeverity.Warning);
                    return;
                }

                spendingLimit = parsedLimit;
            }

            await ExecuteCardUpdateAsync(() => _cardApiService.UpdateSettingsAsync(SelectedCard.Id, new UpdateCardSettingsRequest
            {
                SpendingLimit = spendingLimit,
                IsOnlinePaymentsEnabled = SelectedCard.IsOnlinePaymentsEnabled,
                IsContactlessPaymentsEnabled = SelectedCard.IsContactlessPaymentsEnabled
            }));
        }

        private async Task ExecuteCardUpdateAsync(Func<Task<CardCommandResponse?>> operation)
        {
            try
            {
                IsLoading = true;
                CardCommandResponse? response = await operation();
                if (response?.Success != true)
                {
                    ShowStatus(response?.Message ?? "Card update failed.", InfoBarSeverity.Error);
                    return;
                }

                if (response.Card != null)
                {
                    ReplaceCard(response.Card);
                }

                ShowStatus(response.Message, InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus($"Card update failed: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ReplaceCard(CardSummaryDto updatedCard)
        {
            int index = Cards.ToList().FindIndex(card => card.Id == updatedCard.Id);
            if (index >= 0)
            {
                Cards[index] = updatedCard;
                SelectedCard = Cards[index];
                return;
            }

            Cards.Add(updatedCard);
            SelectedCard = updatedCard;
        }

        private void StartAutoHideCountdown(int durationSeconds)
        {
            _autoHideCancellation?.Cancel();
            _autoHideCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _autoHideCancellation.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    for (int remainingSeconds = durationSeconds; remainingSeconds > 0; remainingSeconds--)
                    {
                        RunOnUiThread(() => RevealCountdownText = $"Auto-hide in {remainingSeconds}s");
                        await Task.Delay(1000, cancellationToken);
                    }

                    RunOnUiThread(HideSensitiveDetails);
                }
                catch (OperationCanceledException)
                {
                    // Ignored because a new reveal action replaced the countdown.
                }
            }, cancellationToken);
        }

        private void HideSensitiveDetails()
        {
            _autoHideCancellation?.Cancel();
            RevealedCardNumber = string.Empty;
            RevealedCvv = string.Empty;
            RevealCountdownText = string.Empty;
            IsSensitiveDetailsVisible = false;
        }

        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            StatusMessage = message;
            StatusSeverity = severity;
            IsStatusOpen = !string.IsNullOrWhiteSpace(message);
        }

        private void RaiseCardActionStateChanged()
        {
            _freezeCommand.RaiseCanExecuteChanged();
            _unfreezeCommand.RaiseCanExecuteChanged();
            _saveSettingsCommand.RaiseCanExecuteChanged();
        }

        public override void Dispose()
        {
            _autoHideCancellation?.Cancel();
            _autoHideCancellation?.Dispose();
        }
    }

    public class SelectableOption
    {
        public SelectableOption(string value, string label)
        {
            Value = value;
            Label = label;
        }

        public string Value { get; }

        public string Label { get; }
    }
}
