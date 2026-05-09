using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Cards;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class CardService : ICardService
    {
        private readonly ICardRepoProxy _repoProxy;

        public CardService(ICardRepoProxy repoProxy)
        {
            _repoProxy = repoProxy;
        }

        public Task<GetCardsResponse?> GetCardsAsync() => _repoProxy.GetCardsAsync();
        public Task<CardDetailsResponse?> GetCardAsync(int cardId) => _repoProxy.GetCardAsync(cardId);
        public Task<RevealCardResponse?> RevealCardAsync(int cardId, RevealCardRequest request) => _repoProxy.RevealCardAsync(cardId, request);
        public Task<CardCommandResponse?> FreezeCardAsync(int cardId) => _repoProxy.FreezeCardAsync(cardId);
        public Task<CardCommandResponse?> UnfreezeCardAsync(int cardId) => _repoProxy.UnfreezeCardAsync(cardId);
        public Task<CardCommandResponse?> UpdateSettingsAsync(int cardId, UpdateCardSettingsRequest request) => _repoProxy.UpdateSettingsAsync(cardId, request);
        public Task<CardCommandResponse?> UpdateSortPreferenceAsync(UpdateCardSortPreferenceRequest request) => _repoProxy.UpdateSortPreferenceAsync(request);
    }
}

