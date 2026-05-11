using BankApp.Models.DTOs.Cards;
using BankApp.Models.Entities;
using BankApp.Server.Configuration;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BankApp.Server.Services.Implementations
{
    public class CardService : ICardService
    {
        private const string ActiveCardStatus = "Active";
        private const string FrozenCardStatus = "Frozen";

        private readonly ICardRepository _cardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHashService _hashService;
        private readonly IOTPService _otpService;
        private readonly IEmailService _emailService;
        private readonly TeamCOptions _options;

        public CardService(
            ICardRepository cardRepository,
            IUserRepository userRepository,
            IHashService hashService,
            IOTPService otpService,
            IEmailService emailService,
            IOptions<TeamCOptions> options)
        {
            this._cardRepository = cardRepository;
            this._userRepository = userRepository;
            this._hashService = hashService;
            this._otpService = otpService;
            this._emailService = emailService;
            this._options = options.Value;
        }

        public GetCardsResponse GetCards(int userId)
        {
            List<Card> cards = _cardRepository.GetCardsByUserId(userId);
            string sortOption = NormalizeSortOption(_cardRepository.GetSortPreference(userId)?.SortOption);

            return new GetCardsResponse
            {
                Success = true,
                Message = "Cards loaded successfully.",
                SortOption = sortOption,
                Cards = SortCards(cards, sortOption).Select(MapToSummary).ToList()
            };
        }

        public CardDetailsResponse GetCard(int userId, int cardId)
        {
            Card? card = GetOwnedCard(userId, cardId);
            if (card == null)
            {
                return new CardDetailsResponse
                {
                    Success = false,
                    Message = "Card not found."
                };
            }

            return new CardDetailsResponse
            {
                Success = true,
                Message = "Card loaded successfully.",
                Card = MapToSummary(card)
            };
        }

        public RevealCardResponse RevealSensitiveDetails(int userId, int cardId, RevealCardRequest request)
        {
            User? user = _userRepository.FindById(userId);
            Card? card = GetOwnedCard(userId, cardId);

            if (user == null)
            {
                return new RevealCardResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (card == null)
            {
                return new RevealCardResponse
                {
                    Success = false,
                    Message = "Card not found."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Password) || !_hashService.Verify(request.Password, user.PasswordHash))
            {
                return new RevealCardResponse
                {
                    Success = false,
                    Message = "Password verification failed."
                };
            }

            if (user.Is2FAEnabled)
            {
                if (string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    SendRevealOtp(user);
                    return new RevealCardResponse
                    {
                        Success = false,
                        RequiresOtp = true,
                        Message = "OTP verification is required before revealing card details.",
                        RevealDurationSeconds = _options.CardRevealDurationSeconds
                    };
                }

                if (!_otpService.VerifyTOTP(user.Id, request.OtpCode))
                {
                    return new RevealCardResponse
                    {
                        Success = false,
                        Message = "Invalid or expired OTP code."
                    };
                }

                _otpService.InvalidateOTP(user.Id);
            }

            return new RevealCardResponse
            {
                Success = true,
                Message = "Sensitive card details revealed successfully.",
                RevealDurationSeconds = _options.CardRevealDurationSeconds,
                SensitiveDetails = new SensitiveCardDetailsDto
                {
                    CardNumber = card.CardNumber,
                    Cvv = card.CVV
                }
            };
        }

        public CardCommandResponse FreezeCard(int userId, int cardId)
        {
            return ChangeCardStatus(userId, cardId, FrozenCardStatus, "Card frozen successfully.");
        }

        public CardCommandResponse UnfreezeCard(int userId, int cardId)
        {
            return ChangeCardStatus(userId, cardId, ActiveCardStatus, "Card unfrozen successfully.");
        }

        public CardCommandResponse UpdateSettings(int userId, int cardId, UpdateCardSettingsRequest request)
        {
            Card? card = GetOwnedCard(userId, cardId);
            if (card == null)
            {
                return CreateCommandFailure("Card not found.");
            }

            if (request.SpendingLimit.HasValue)
            {
                if (request.SpendingLimit.Value < 0)
                {
                    return CreateCommandFailure("Spending limit must be a non-negative value.");
                }

                if (request.SpendingLimit.Value > _options.MaximumSpendingLimit)
                {
                    return CreateCommandFailure($"Spending limit cannot exceed {_options.MaximumSpendingLimit:0.##}.");
                }
            }

            decimal? spendingLimit = request.SpendingLimit ?? card.MonthlySpendingCap;
            bool isOnlineEnabled = request.IsOnlinePaymentsEnabled ?? card.IsOnlineEnabled;
            bool isContactlessEnabled = request.IsContactlessPaymentsEnabled ?? card.IsContactlessEnabled;

            bool updated = _cardRepository.UpdateSettings(cardId, spendingLimit, isOnlineEnabled, isContactlessEnabled);
            if (!updated)
            {
                return CreateCommandFailure("Failed to update card settings.");
            }

            Card refreshedCard = _cardRepository.GetCardById(cardId) !;
            return new CardCommandResponse
            {
                Success = true,
                Message = "Card settings updated successfully.",
                Card = MapToSummary(refreshedCard)
            };
        }

        public CardCommandResponse UpdateSortPreference(int userId, UpdateCardSortPreferenceRequest request)
        {
            string sortOption = NormalizeSortOption(request.SortOption);
            if (!IsValidSortOption(sortOption))
            {
                return CreateCommandFailure("Unsupported card sort option.");
            }

            bool updated = _cardRepository.SaveSortPreference(userId, sortOption);
            if (!updated)
            {
                return CreateCommandFailure("Failed to update card sort preference.");
            }

            return new CardCommandResponse
            {
                Success = true,
                Message = "Card sort preference updated successfully."
            };
        }

        private CardCommandResponse ChangeCardStatus(int userId, int cardId, string status, string successMessage)
        {
            Card? card = GetOwnedCard(userId, cardId);
            if (card == null)
            {
                return CreateCommandFailure("Card not found.");
            }

            if (string.Equals(card.Status, status, StringComparison.OrdinalIgnoreCase))
            {
                return new CardCommandResponse
                {
                    Success = true,
                    Message = successMessage,
                    Card = MapToSummary(card)
                };
            }

            bool updated = _cardRepository.UpdateStatus(cardId, status);
            if (!updated)
            {
                return CreateCommandFailure("Failed to update card status.");
            }

            Card refreshedCard = _cardRepository.GetCardById(cardId) !;
            return new CardCommandResponse
            {
                Success = true,
                Message = successMessage,
                Card = MapToSummary(refreshedCard)
            };
        }

        private Card? GetOwnedCard(int userId, int cardId)
        {
            Card? card = _cardRepository.GetCardById(cardId);
            return card != null && card.User?.Id == userId ? card : null;
        }

        private CardSummaryDto MapToSummary(Card card)
        {
            Account? account = card.Account;

            return new CardSummaryDto
            {
                Id = card.Id,
                AccountId = account?.Id ?? 0,
                AccountName = account?.AccountName ?? $"Account {account?.Id ?? 0}",
                AccountIban = account?.IBAN ?? string.Empty,
                MaskedCardNumber = MaskCardNumber(card.CardNumber),
                CardholderName = card.CardholderName,
                ExpiryDate = card.ExpiryDate,
                CardType = card.CardType,
                CardBrand = card.CardBrand ?? string.Empty,
                Status = card.Status,
                SpendingLimit = card.MonthlySpendingCap,
                IsOnlinePaymentsEnabled = card.IsOnlineEnabled,
                IsContactlessPaymentsEnabled = card.IsContactlessEnabled,
                SortOrder = card.SortOrder
            };
        }

        private IEnumerable<Card> SortCards(IEnumerable<Card> cards, string sortOption)
        {
            return sortOption switch
            {
                CardSortOptions.CardholderName => cards.OrderBy(card => card.CardholderName, StringComparer.OrdinalIgnoreCase),
                CardSortOptions.ExpiryDate => cards.OrderBy(card => card.ExpiryDate),
                CardSortOptions.Status => cards.OrderBy(card => card.Status, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(card => card.ExpiryDate),
                _ => cards.OrderBy(card => card.SortOrder).ThenBy(card => card.CreatedAt)
            };
        }

        private void SendRevealOtp(User user)
        {
            string otp = _otpService.GenerateTOTP(user.Id);
            if (string.IsNullOrWhiteSpace(user.Preferred2FAMethod) ||
                string.Equals(user.Preferred2FAMethod, "Email", StringComparison.OrdinalIgnoreCase))
            {
                _emailService.SendOTPCode(user.Email, otp);
            }
        }

        private static string NormalizeSortOption(string? sortOption)
        {
            if (string.IsNullOrWhiteSpace(sortOption))
            {
                return CardSortOptions.Custom;
            }

            return sortOption.Trim();
        }

        private static bool IsValidSortOption(string sortOption)
        {
            return sortOption == CardSortOptions.Custom ||
                   sortOption == CardSortOptions.CardholderName ||
                   sortOption == CardSortOptions.ExpiryDate ||
                   sortOption == CardSortOptions.Status;
        }

        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            {
                return "****";
            }

            return $"**** {cardNumber[^4..]}";
        }

        private static CardCommandResponse CreateCommandFailure(string message)
        {
            return new CardCommandResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}
