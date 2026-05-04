﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BankApp.Models.Features.Chat
{
    public class ChatSession : INotifyPropertyChanged
    {
        private const int NoMessagesCount = 0;

        public int Id { get; set; }

        public int UserId { get; set; }

        public string IssueCategory { get; set; } = string.Empty;

        public string SessionStatus { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Feedback { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; }

        public DateTime EndedAt { get; set; }

        private string title = "New chat";
        private string lastPreview = "No messages yet.";
        private DateTime lastUpdatedAt = DateTime.Now;
        private bool isEscalatedToTeam;
        private string teamContactMessage = string.Empty;
        private SelectedAttachment? attachment;

        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

        public string Title
        {
            get => $"Chat {Id}";
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastPreview
        {
            get => Messages.Count > NoMessagesCount ? Messages.Last().Content : "No messages yet.";
            set
            {
                if (lastPreview != value)
                {
                    lastPreview = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastUpdatedAt
        {
            get => lastUpdatedAt;
            set
            {
                if (lastUpdatedAt != value)
                {
                    lastUpdatedAt = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastUpdatedDisplay));
                }
            }
        }

        public string LastUpdatedDisplay => LastUpdatedAt.ToString("g");

        public bool IsEscalatedToTeam
        {
            get => isEscalatedToTeam;
            set
            {
                if (isEscalatedToTeam != value)
                {
                    isEscalatedToTeam = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SessionModeLabel));
                }
            }
        }

        public string SessionModeLabel => IsEscalatedToTeam ? "Team contact" : "Chatbot assistance";

        public string TeamContactMessage
        {
            get => teamContactMessage;
            set
            {
                if (teamContactMessage != value)
                {
                    teamContactMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public SelectedAttachment? Attachment
        {
            get => attachment;
            set
            {
                if (attachment != value)
                {
                    attachment = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}