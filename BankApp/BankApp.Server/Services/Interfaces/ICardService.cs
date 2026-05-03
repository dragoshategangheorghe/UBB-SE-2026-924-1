using BankApp.Models.DTOs.Cards;

namespace BankApp.Server.Services.Interfaces
{
    public interface ICardService
    {
        GetCardsResponse GetCards(int userId);
        CardDetailsResponse GetCard(int userId, int cardId);
        RevealCardResponse RevealSensitiveDetails(int userId, int cardId, RevealCardRequest request);
        CardCommandResponse FreezeCard(int userId, int cardId);
        CardCommandResponse UnfreezeCard(int userId, int cardId);
        CardCommandResponse UpdateSettings(int userId, int cardId, UpdateCardSettingsRequest request);
        CardCommandResponse UpdateSortPreference(int userId, UpdateCardSortPreferenceRequest request);
    }
}
