using System;
using System.Collections.Generic;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Dashboard;
using BankApp.Models.Entities;
using BankApp.Models.Enums;

namespace BankApp.Client.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public Observable<DashboardState> State { get; private set; }
        public User CurrentUser { get; private set; }
        public List<Card> Cards { get; private set; }
        public List<Transaction> RecentTransactions { get; private set; }
        public int UnreadNotificationCount { get; private set; }

        private readonly IDashboardService _dashboardService;

        public DashboardViewModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
            State = new Observable<DashboardState>(DashboardState.Loading);
            Cards = new List<Card>();
            RecentTransactions = new List<Transaction>();
            UnreadNotificationCount = 0;
        }

        public async void LoadDashboard()
        {
            SetState(State, DashboardState.Loading);
            try
            {
                DashboardResponse? response = await _dashboardService.GetDashboardAsync();

                if (response == null)
                {
                    SetState(State, DashboardState.Error);
                    return;
                }

                CurrentUser = response.CurrentUser;
                Cards = response.Cards;
                RecentTransactions = response.RecentTransactions;
                UnreadNotificationCount = response.UnreadNotificationCount;
                SetState(State, DashboardState.Success);
            }
            catch (Exception)
            {
                SetState(State, DashboardState.Error);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
        }
    }
}