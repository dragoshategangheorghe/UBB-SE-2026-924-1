using System;
using System.Collections.Generic;
using System.Linq;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Chat;
using BankApp.Models.Features.Chat;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    public sealed partial class ChatRoutingView : Page
    {
        private readonly IChatService chatService;

        public ChatRoutingView()
        {
            InitializeComponent();
            chatService = App.ChatService;
            Loaded += ChatRoutingView_Loaded;
        }

        private async void ChatRoutingView_Loaded(object sender, RoutedEventArgs e)
        {
            List<ChatSession>? sessions = await chatService.GetSessionsAsync();
            List<ChatSession> safeSessions = sessions ?? new List<ChatSession>();
            foreach (ChatSession session in safeSessions)
            {
                session.LastUpdatedAt = session.StartedAt;
            }

            WaitTimeText.Text = $"Estimated wait time: {Math.Max(1, safeSessions.Count + 1)} minute(s)";
            SessionsList.ItemsSource = safeSessions;
        }

        private async void StartNewChat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string issueCategory = IssueCategoryComboBox.SelectedItem?.ToString() ?? "General";
                CreateChatSessionResponse? response = await chatService.CreateSessionAsync(issueCategory);
                if (response == null || !response.Success || response.SessionId <= 0)
                {
                    return;
                }

                Frame?.Navigate(typeof(ChatView), response.SessionId);
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Could not start chat",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void SessionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ChatSession session)
            {
                Frame?.Navigate(typeof(ChatView), session.Id);
            }
        }
    }
}