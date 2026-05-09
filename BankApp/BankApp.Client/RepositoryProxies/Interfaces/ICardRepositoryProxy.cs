using BankApp.Models.DTOs.Cards;
using BankApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Interfaces
{
    public interface ICardRepositoryProxy
    {
        Task<List<Card>?> GetCardsAsync();
        Task<Card?> GetCardAsync(int cardId);
        Task<Account?> GetAccountAsync(int accountId);
        Task<UserCardPreference?> GetSortPreferenceAsync();
        Task<bool> SaveSortPreferencesAsync(string sortOption);
        Task<bool> UpdateStatus(int cardId, string status);
        Task<bool> UpdateStatus(int cardId, UpdateCardSettingsRequest request);
    }
}
