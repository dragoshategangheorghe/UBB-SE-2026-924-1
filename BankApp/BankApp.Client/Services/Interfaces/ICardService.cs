using BankApp.Models.DTOs.Cards;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ICardService
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

