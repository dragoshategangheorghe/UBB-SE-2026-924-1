using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Cards;
using BankApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class CardRepositoryProxy : ICardRepositoryProxy
    {
        private readonly ApiService apiService;

        public CardRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<List<Card>?> GetCardsAsync()
        {
            return this.apiService.GetAsync<List<Card>>("/api/cards");
        }

        public Task<Card?> GetCardAsync(int cardId)
        {
            return this.apiService.GetAsync<Card>($"/api/cards/{cardId}");
        }

        public Task<Account?> GetAccountAsync(int accountId)
        {
            return this.apiService.GetAsync<Account>($"/api/cards/account/{accountId}");
        }

        public Task<UserCardPreference?> GetSortPreferenceAsync()
        {
            return this.apiService.GetAsync<UserCardPreference>($"/api/cards/sortPreference");
        }

        public Task<bool> SaveSortPreferencesAsync(string sortOption)
        {
            return this.apiService.PutAsync<UpdateCardSortPreferenceRequest, bool>($"/api/cards/sortPreference/${sortOption}", new UpdateCardSortPreferenceRequest { });
        }

        public Task<bool> UpdateStatus(int cardId, string status)
        {
            return this.apiService.PutAsync<UpdateCardSettingsRequest, bool>($"/api/cards/{cardId}/updateStatus/{status}", new UpdateCardSettingsRequest { });
        }

        public Task<bool> UpdateStatus(int cardId, UpdateCardSettingsRequest request)
        {
            return this.apiService.PostAsync<UpdateCardSettingsRequest, bool>($"/api/cards/{cardId}/updateSettings", request);
        }
    }
}
