using System.Threading.Tasks;
using BankApp.Models.DTOs.Cards;

namespace BankApp.Client.Services.Interfaces
{
    public interface ICardApiService
    {
        Task<GetCardsResponse?> GetCardsAsync();
        Task<CardDetailsResponse?> GetCardAsync(int cardId);
        Task<RevealCardResponse?> RevealCardAsync(int cardId, RevealCardRequest request);
        Task<CardCommandResponse?> FreezeCardAsync(int cardId);
        Task<CardCommandResponse?> UnfreezeCardAsync(int cardId);
        Task<CardCommandResponse?> UpdateSettingsAsync(int cardId, UpdateCardSettingsRequest request);
        Task<CardCommandResponse?> UpdateSortPreferenceAsync(UpdateCardSortPreferenceRequest request);
    }
}
