using System;
using System.Collections.Generic;
using System.Linq;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Chat;
using BankApp.Models.Features.Chat;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BankApp.Client.Views
{
    public sealed partial class ChatView : Page
    {
        private const int MaxAttachmentSizeBytes = 10 * 1024 * 1024;
        private static readonly Dictionary<string, string> DefaultChatbotResponses = new Dictionary<string, string>
        {
            ["How do I reset my password?"] =
                "You can reset your password from the login screen by choosing Forgot password and following the verification steps.",
            ["Why was my card declined?"] =
                "A card can be declined because of insufficient funds, an expired card, a blocked card, or a merchant validation issue. Please check the card status in the app first.",
            ["How long does a transfer take?"] =
                "Internal transfers are usually immediate, while interbank transfers can take up to one business day depending on the destination bank.",
            ["How do I upload documents for support?"] =
                "Use the Attach File button in this chat after contacting the team. Your selected file will be included with the support request summary.",
            ["I found a technical problem in the app."] =
                "Please contact the team from this chat and include a short description of what happened. Screenshots or PDFs can help the team investigate faster.",
        };

        private readonly IChatService chatService;
        private readonly DispatcherTimer refreshTimer;
        private int sessionId;
        private StorageFile? pendingAttachment;

        public ChatView()
        {
            InitializeComponent();
            chatService = App.ChatService;
            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            refreshTimer.Tick += RefreshTimer_Tick;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int parameterSessionId)
            {
                sessionId = parameterSessionId;
            }

            if (sessionId <= 0)
            {
                return;
            }

            try
            {
                ChatSession? session = await chatService.GetSessionAsync(sessionId);
                HeaderText.Text = session == null
                    ? $"Chat #{sessionId}"
                    : $"{session.IssueCategory} - #{session.Id}";

                await LoadMessagesAsync();
                refreshTimer.Start();
            }
            catch (Exception ex)
            {
                HeaderText.Text = $"Chat #{sessionId}";
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Could not load chat",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            refreshTimer.Stop();
        }

        private async System.Threading.Tasks.Task LoadMessagesAsync()
        {
            List<ChatMessage>? messages = await chatService.GetMessagesAsync(sessionId);
            MessagesList.ItemsSource = messages ?? new List<ChatMessage>();
            if (messages != null && messages.Count > 0)
            {
                MessagesList.ScrollIntoView(messages.Last());
            }
        }

        private async void AskPresetQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            string content = PresetQuestionsComboBox.SelectedItem?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content) || sessionId <= 0)
            {
                return;
            }

            CreateChatMessageResponse? response = await chatService.CreateMessageAsync(sessionId, "User", content);
            if (response == null || !response.Success)
            {
                return;
            }

            if (pendingAttachment != null)
            {
                BasicProperties props = await pendingAttachment.GetBasicPropertiesAsync();
                string extension = System.IO.Path.GetExtension(pendingAttachment.Name)?.ToLowerInvariant() ?? string.Empty;
                string fileType = extension == ".pdf" ? "application/pdf" : "image";

                await chatService.CreateAttachmentAsync(response.MessageId, new CreateChatAttachmentRequest
                {
                    AttachmentName = pendingAttachment.Name,
                    FileType = fileType,
                    FileSizeBytes = (int)props.Size,
                    StorageUrl = pendingAttachment.Path
                });
            }

            PresetQuestionsComboBox.SelectedItem = null;
            pendingAttachment = null;
            AttachmentText.Text = string.Empty;
            await LoadMessagesAsync();
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainAppWindow == null)
            {
                return;
            }

            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".pdf");
            nint windowHandle = WindowNative.GetWindowHandle(App.MainAppWindow);
            InitializeWithWindow.Initialize(picker, windowHandle);

            StorageFile? file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            string extension = System.IO.Path.GetExtension(file.Name)?.ToLowerInvariant() ?? string.Empty;
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".pdf")
            {
                AttachmentText.Text = "Only images or PDF files are allowed.";
                pendingAttachment = null;
                return;
            }

            BasicProperties properties = await file.GetBasicPropertiesAsync();
            if (properties.Size > MaxAttachmentSizeBytes)
            {
                AttachmentText.Text = "Attachment must be up to 10 MB.";
                pendingAttachment = null;
                return;
            }

            pendingAttachment = file;
            AttachmentText.Text = $"{file.Name} ({properties.Size / 1024} KB)";
        }

        private async void EscalateButton_Click(object sender, RoutedEventArgs e)
        {
            await chatService.UpdateSessionStatusAsync(sessionId, "Escalated");
            await LoadMessagesAsync();
        }

        private async void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            await chatService.UpdateSessionStatusAsync(sessionId, "Closed");

            ComboBox ratingBox = new ComboBox
            {
                PlaceholderText = "Select rating (1-5)"
            };
            ratingBox.Items.Add(1);
            ratingBox.Items.Add(2);
            ratingBox.Items.Add(3);
            ratingBox.Items.Add(4);
            ratingBox.Items.Add(5);

            TextBox feedbackBox = new TextBox
            {
                PlaceholderText = "Optional written feedback."
            };

            ContentDialog dialog = new ContentDialog
            {
                Title = "Rate your support experience",
                Content = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        ratingBox,
                        feedbackBox
                    }
                },
                PrimaryButtonText = "Submit",
                CloseButtonText = "Skip",
                XamlRoot = XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (ratingBox.SelectedItem is int rating)
                {
                    await chatService.SaveFeedbackAsync(sessionId, rating, feedbackBox.Text ?? string.Empty);
                }
            }
        }

        private async void EmailTranscriptButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Email chat transcript",
                Content = new TextBox
                {
                    PlaceholderText = "Enter destination email"
                },
                PrimaryButtonText = "Send",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.Content is TextBox emailBox)
            {
                await chatService.EmailTranscriptAsync(sessionId, emailBox.Text ?? string.Empty);
            }
        }

        private async void RefreshTimer_Tick(object? sender, object e)
        {
            await LoadMessagesAsync();
        }
    }
}