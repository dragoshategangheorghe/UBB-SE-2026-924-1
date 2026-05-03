using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Cards;

namespace BankApp.Client.Services.Implementations
{
    public class CardApiService : ICardApiService
    {
        private readonly ApiService _apiService;

        public CardApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<GetCardsResponse?> GetCardsAsync()
        {
            return _apiService.GetAsync<GetCardsResponse>("api/cards");
        }

        public Task<CardDetailsResponse?> GetCardAsync(int cardId)
        {
            return _apiService.GetAsync<CardDetailsResponse>($"api/cards/{cardId}");
        }

        public Task<RevealCardResponse?> RevealCardAsync(int cardId, RevealCardRequest request)
        {
            return _apiService.PostAsync<RevealCardRequest, RevealCardResponse>($"api/cards/{cardId}/reveal", request);
        }

        public Task<CardCommandResponse?> FreezeCardAsync(int cardId)
        {
            return _apiService.PutAsync<object, CardCommandResponse>($"api/cards/{cardId}/freeze", new { });
        }

        public Task<CardCommandResponse?> UnfreezeCardAsync(int cardId)
        {
            return _apiService.PutAsync<object, CardCommandResponse>($"api/cards/{cardId}/unfreeze", new { });
        }

        public Task<CardCommandResponse?> UpdateSettingsAsync(int cardId, UpdateCardSettingsRequest request)
        {
            return _apiService.PutAsync<UpdateCardSettingsRequest, CardCommandResponse>($"api/cards/{cardId}/settings", request);
        }

        public Task<CardCommandResponse?> UpdateSortPreferenceAsync(UpdateCardSortPreferenceRequest request)
        {
            return _apiService.PutAsync<UpdateCardSortPreferenceRequest, CardCommandResponse>("api/cards/preferences/sort", request);
        }
    }
}
