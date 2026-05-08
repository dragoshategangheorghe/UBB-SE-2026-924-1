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

        private async void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            // Build dialog inputs
            var accountIdBox = new TextBox { PlaceholderText = "Account ID (numeric)" };
            var cardholderBox = new TextBox { PlaceholderText = "Cardholder Name" };
            var expiryPicker = new DatePicker { Date = DateTime.Now.AddYears(3) };
            var typeBox = new ComboBox { ItemsSource = new[] { "Physical", "Virtual" }, SelectedIndex = 0 };
            var brandBox = new TextBox { PlaceholderText = "Card Brand (optional)" };
            var numberBox = new TextBox { PlaceholderText = "Card Number (optional)" };
            var cvvBox = new TextBox { PlaceholderText = "CVV (optional)" };
            var monthlyCapBox = new TextBox { PlaceholderText = "Monthly spending cap (optional)" };
            var onlineToggle = new ToggleSwitch { Header = "Online Payments", IsOn = true };
            var contactlessToggle = new ToggleSwitch { Header = "Contactless Payments", IsOn = true };

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = "Account ID" });
            panel.Children.Add(accountIdBox);
            panel.Children.Add(new TextBlock { Text = "Cardholder Name" });
            panel.Children.Add(cardholderBox);
            panel.Children.Add(new TextBlock { Text = "Expiry Date" });
            panel.Children.Add(expiryPicker);
            panel.Children.Add(new TextBlock { Text = "Card Type" });
            panel.Children.Add(typeBox);
            panel.Children.Add(new TextBlock { Text = "Card Brand" });
            panel.Children.Add(brandBox);
            panel.Children.Add(new TextBlock { Text = "Card Number" });
            panel.Children.Add(numberBox);
            panel.Children.Add(new TextBlock { Text = "CVV" });
            panel.Children.Add(cvvBox);
            panel.Children.Add(new TextBlock { Text = "Monthly Spending Cap" });
            panel.Children.Add(monthlyCapBox);
            panel.Children.Add(onlineToggle);
            panel.Children.Add(contactlessToggle);

            ContentDialog dialog = new()
            {
                Title = "Create New Card",
                Content = panel,
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await ShowDialogAsync(dialog) != ContentDialogResult.Primary)
            {
                return;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(cardholderBox.Text))
            {
                await ShowMessageAsync("Validation", "Cardholder name is required.");
                return;
            }

            if (!int.TryParse(accountIdBox.Text, out int accountId))
            {
                await ShowMessageAsync("Validation", "Account ID must be a valid integer.");
                return;
            }

            if (typeBox.SelectedItem == null)
            {
                await ShowMessageAsync("Validation", "Card type is required.");
                return;
            }

            DateTime expiry = expiryPicker.Date.DateTime;

            decimal? monthlyCap = null;
            if (!string.IsNullOrWhiteSpace(monthlyCapBox.Text))
            {
                if (decimal.TryParse(monthlyCapBox.Text, out decimal parsedCap))
                {
                    monthlyCap = parsedCap;
                }
                else
                {
                    await ShowMessageAsync("Validation", "Monthly spending cap must be a valid decimal number.");
                    return;
                }
            }

            var request = new CreateCardRequest
            {
                AccountId = accountId,
                CardholderName = cardholderBox.Text.Trim(),
                ExpiryDate = expiry,
                CardType = typeBox.SelectedItem?.ToString() ?? string.Empty,
                CardBrand = string.IsNullOrWhiteSpace(brandBox.Text) ? null : brandBox.Text.Trim(),
                CardNumber = string.IsNullOrWhiteSpace(numberBox.Text) ? null : numberBox.Text.Trim(),
                Cvv = string.IsNullOrWhiteSpace(cvvBox.Text) ? null : cvvBox.Text.Trim(),
                MonthlySpendingCap = monthlyCap,
                IsOnlinePaymentsEnabled = onlineToggle.IsOn,
                IsContactlessPaymentsEnabled = contactlessToggle.IsOn
            };

            var response = await ViewModel.CreateCardAsync(request);
            if (response == null)
            {
                await ShowMessageAsync("Create Card", "Failed to create card.");
                return;
            }

            if (response.Success)
            {
                await ShowMessageAsync("Create Card", response.Message ?? "Card created successfully.");
            }
            else
            {
                await ShowMessageAsync("Create Card", response.Message ?? "Failed to create card.");
            }
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
