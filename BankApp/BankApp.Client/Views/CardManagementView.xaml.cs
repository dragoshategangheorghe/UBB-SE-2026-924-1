using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.ViewModels;
using BankApp.Models.DTOs.Cards;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;

namespace BankApp.Client.Views
{
    public sealed partial class CardManagementView : Page
    {
        public CardManagementView()
        {
            InitializeComponent();
            ViewModel = new CardManagementViewModel(App.CardApiService);
            DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Loaded += CardManagementView_Loaded;
            Unloaded += CardManagementView_Unloaded;
        }

        public CardManagementViewModel ViewModel { get; }

        private async void CardManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadAsync();
            CardSummaryDto? initialCard =
                CardsList.SelectedItem as CardSummaryDto ??
                ViewModel.SelectedCard ??
                ViewModel.Cards.FirstOrDefault();

            if (initialCard != null)
            {
                CardsList.SelectedItem = initialCard;
            }

            SetSelectedCard(initialCard);
        }

        private void CardManagementView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.Dispose();
        }

        private void CardsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                SetSelectedCard(listView.SelectedItem as CardSummaryDto);
            }
        }

        private void CardsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            SetSelectedCard(e.ClickedItem as CardSummaryDto);
        }

        private void OnlinePaymentsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCard != null)
            {
                ViewModel.SelectedCard.IsOnlinePaymentsEnabled = OnlinePaymentsToggle.IsOn;
            }
        }

        private void ContactlessPaymentsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCard != null)
            {
                ViewModel.SelectedCard.IsContactlessPaymentsEnabled = ContactlessPaymentsToggle.IsOn;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CardManagementViewModel.SelectedCard))
            {
                UpdateSelectedCardPanel(ViewModel.SelectedCard);
            }

            if (e.PropertyName == nameof(CardManagementViewModel.RevealedCardNumber))
            {
                SensitiveCardNumberText.Text = ViewModel.RevealedCardNumber;
            }

            if (e.PropertyName == nameof(CardManagementViewModel.RevealedCvv))
            {
                SensitiveCvvText.Text = ViewModel.RevealedCvv;
            }

            if (e.PropertyName == nameof(CardManagementViewModel.RevealCountdownText))
            {
                RevealCountdownTextBlock.Text = ViewModel.RevealCountdownText;
            }

            if (e.PropertyName == nameof(CardManagementViewModel.IsSensitiveDetailsVisible))
            {
                SensitiveDetailsBorder.Visibility = ViewModel.IsSensitiveDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SetSelectedCard(CardSummaryDto? card)
        {
            ViewModel.SelectedCard = card;
            UpdateSelectedCardPanel(card);
        }

        private void UpdateSelectedCardPanel(CardSummaryDto? card)
        {
            bool hasCard = card != null;
            EmptyCardText.Visibility = hasCard ? Visibility.Collapsed : Visibility.Visible;
            SelectedCardDetailsPanel.Visibility = hasCard ? Visibility.Visible : Visibility.Collapsed;

            CardholderNameText.Text = card?.CardholderName ?? string.Empty;
            MaskedCardNumberText.Text = MaskCardNumberForDisplay(card?.MaskedCardNumber);
            AccountNameText.Text = card?.AccountName ?? string.Empty;
            AccountIbanText.Text = card?.AccountIban ?? string.Empty;
            StatusValueText.Text = card?.Status ?? string.Empty;
            ExpiryValueText.Text = card == null ? string.Empty : card.ExpiryDate.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);

            OnlinePaymentsToggle.IsOn = card?.IsOnlinePaymentsEnabled ?? false;
            ContactlessPaymentsToggle.IsOn = card?.IsContactlessPaymentsEnabled ?? false;

            SpendingLimitBox.IsEnabled = hasCard;
            OnlinePaymentsToggle.IsEnabled = hasCard;
            ContactlessPaymentsToggle.IsEnabled = hasCard;
            FreezeCardButton.IsEnabled = hasCard;
            UnfreezeCardButton.IsEnabled = hasCard;
            SaveSettingsButton.IsEnabled = hasCard;
            RevealDetailsButton.IsEnabled = hasCard;
        }

        private static string MaskCardNumberForDisplay(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return string.Empty;
            }

            string digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length < 4)
            {
                return "****";
            }

            return $"**** **** **** {digitsOnly[^4..]}";
        }

        private async void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            string? password = await PromptForPasswordAsync();
            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var response = await ViewModel.RevealSensitiveDetailsAsync(password, null);
            if (response?.RequiresOtp != true)
            {
                await HandleRevealResponseAsync(response);
                return;
            }

            string? otpCode = await PromptForOtpAsync();
            if (string.IsNullOrWhiteSpace(otpCode))
            {
                return;
            }

            RevealCardResponse? otpResponse = await ViewModel.RevealSensitiveDetailsAsync(password, otpCode);
            await HandleRevealResponseAsync(otpResponse);
        }

        private async Task HandleRevealResponseAsync(RevealCardResponse? response)
        {
            if (response == null)
            {
                return;
            }

            if (response.Success)
            {
                if (response.SensitiveDetails != null)
                {
                    SensitiveCardNumberText.Text = response.SensitiveDetails.CardNumber;
                    SensitiveCvvText.Text = response.SensitiveDetails.Cvv;
                    SensitiveDetailsBorder.Visibility = Visibility.Visible;
                }

                await Task.Delay(50);
                SensitiveDetailsBorder.StartBringIntoView(new BringIntoViewOptions
                {
                    AnimationDesired = true
                });

                if (response.SensitiveDetails != null)
                {
                    await ShowMessageAsync(
                        "Sensitive Details",
                        $"Card Number: {response.SensitiveDetails.CardNumber}\nCVV: {response.SensitiveDetails.Cvv}");
                }
                return;
            }

            if (!response.RequiresOtp && !string.IsNullOrWhiteSpace(response.Message))
            {
                await ShowMessageAsync("Reveal Details", response.Message);
            }
        }

        private async Task<string?> PromptForPasswordAsync()
        {
            PasswordBox passwordBox = new();
            ContentDialog dialog = new()
            {
                Title = "Confirm Password",
                Content = passwordBox,
                PrimaryButtonText = "Continue",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            return await ShowDialogAsync(dialog) == ContentDialogResult.Primary ? passwordBox.Password : null;
        }

        private async Task<string?> PromptForOtpAsync()
        {
            TextBox otpBox = new()
            {
                PlaceholderText = "Enter OTP"
            };

            ContentDialog dialog = new()
            {
                Title = "OTP Verification",
                Content = otpBox,
                PrimaryButtonText = "Verify",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            return await ShowDialogAsync(dialog) == ContentDialogResult.Primary ? otpBox.Text : null;
        }

        private static Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
        {
            TaskCompletionSource<ContentDialogResult> taskCompletionSource = new();
            IAsyncOperation<ContentDialogResult> operation = dialog.ShowAsync();
            operation.Completed = (asyncInfo, status) =>
            {
                switch (status)
                {
                    case AsyncStatus.Completed:
                        taskCompletionSource.SetResult(asyncInfo.GetResults());
                        break;
                    case AsyncStatus.Canceled:
                        taskCompletionSource.SetResult(ContentDialogResult.None);
                        break;
                    case AsyncStatus.Error:
                        taskCompletionSource.SetException(asyncInfo.ErrorCode);
                        break;
                }
            };

            return taskCompletionSource.Task;
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            ContentDialog dialog = new()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await ShowDialogAsync(dialog);
        }
    }
}
