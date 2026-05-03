using BankApp.Client.Utilities;

using BankApp.Models.DTOs.Dashboard;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using System;
using System.Collections.Generic;


namespace BankApp.Client.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public Observable<DashboardState> State { get; private set; }
        public User CurrentUser { get; private set; }
        public List<Card> Cards { get; private set; }
        public List<Transaction> RecentTransactions { get; private set; }
        public int UnreadNotificationCount { get; private set; }

        private readonly ApiService _apiService;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
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
                int? userId = _apiService.GetCurrentUserId();
                if (userId == null)
                {
                    SetState(State, DashboardState.Error);
                    return;
                }

                DashboardResponse? response = await _apiService.GetAsync<DashboardResponse>(
                    $"/api/dashboard/");

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

        public override void Dispose() { }
    }
}