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

        private readonly ICardRepository cardRepository;
        private readonly IUserRepository userRepository;
        private readonly IHashService hashService;
        private readonly IOTPService otpService;
        private readonly IEmailService emailService;
        private readonly TeamCOptions options;

        public CardService(
            ICardRepository cardRepository,
            IUserRepository userRepository,
            IHashService hashService,
            IOTPService otpService,
            IEmailService emailService,
            IOptions<TeamCOptions> options)
        {
            this.cardRepository = cardRepository;
            this.userRepository = userRepository;
            this.hashService = hashService;
            this.otpService = otpService;
            this.emailService = emailService;
            this.options = options.Value;
        }

        public GetCardsResponse GetCards(int userId)
        {
            List<Card> cards = cardRepository.GetCardsByUserId(userId);
            string sortOption = NormalizeSortOption(cardRepository.GetSortPreference(userId)?.SortOption);

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

        public CardCommandResponse AddCard(int userId, CreateCardRequest request)
        {
            // Basic validation
            if (request.AccountId <= 0)
            {
                return CreateCommandFailure("AccountId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.CardholderName))
            {
                return CreateCommandFailure("Cardholder name is required.");
            }

            if (request.ExpiryDate <= DateTime.UtcNow.Date)
            {
                return CreateCommandFailure("Expiry date must be in the future.");
            }

            // Check spending cap
            if (request.MonthlySpendingCap.HasValue)
            {
                if (request.MonthlySpendingCap.Value < 0)
                {
                    return CreateCommandFailure("Spending limit must be a non-negative value.");
                }

                if (request.MonthlySpendingCap.Value > options.MaximumSpendingLimit)
                {
                    return CreateCommandFailure($"Spending limit cannot exceed {options.MaximumSpendingLimit:0.##}.");
                }
            }

            // Account existence and ownership
            Account? account = cardRepository.GetAccountById(request.AccountId);
            if (account == null)
            {
                return CreateCommandFailure("Account not found.");
            }

            if (account.User?.Id != userId)
            {
                return CreateCommandFailure("Account does not belong to the authenticated user.");
            }

            // Card number / CVV validation or generation
            string cardNumber = request.CardNumber?.Trim() ?? string.Empty;
            string cvv = request.Cvv?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(cardNumber))
            {
                if (!IsDigitsOnly(cardNumber) || cardNumber.Length < 13 || cardNumber.Length > 19)
                {
                    return CreateCommandFailure("Card number must be 13-19 digits.");
                }

                if (!IsLuhnValid(cardNumber))
                {
                    return CreateCommandFailure("Card number is invalid.");
                }
            }

            if (!string.IsNullOrEmpty(cvv))
            {
                if (!IsDigitsOnly(cvv) || (cvv.Length != 3 && cvv.Length != 4))
                {
                    return CreateCommandFailure("CVV must be 3 or 4 digits.");
                }
            }

            // Generate card number / cvv if not provided
            if (string.IsNullOrEmpty(cardNumber))
            {
                cardNumber = GenerateCardNumber(16);
            }

            if (string.IsNullOrEmpty(cvv))
            {
                cvv = GenerateCvv(3);
            }

            // Determine sort order
            List<Card> existing = cardRepository.GetCardsByUserId(userId);
            int sortOrder = existing.Any() ? existing.Max(c => c.SortOrder) + 1 : 0;

            Card newCard = new Card
            {
                UserId = userId,
                AccountId = request.AccountId,
                CardNumber = cardNumber,
                CardholderName = request.CardholderName.Trim(),
                ExpiryDate = request.ExpiryDate,
                CVV = cvv,
                CardType = request.CardType ?? string.Empty,
                CardBrand = request.CardBrand,
                MonthlySpendingCap = request.MonthlySpendingCap,
                IsOnlineEnabled = request.IsOnlinePaymentsEnabled ?? true,
                IsContactlessEnabled = request.IsContactlessPaymentsEnabled ?? true,
                CreatedAt = DateTime.UtcNow,
                SortOrder = sortOrder,
                Status = ActiveCardStatus
            };

            Card created = cardRepository.CreateCard(newCard);
            if (created == null)
            {
                return CreateCommandFailure("Failed to create card.");
            }

            return new CardCommandResponse
            {
                Success = true,
                Message = "Card created successfully.",
                Card = MapToSummary(created)
            };
        }

        public RevealCardResponse RevealSensitiveDetails(int userId, int cardId, RevealCardRequest request)
        {
            User? user = userRepository.FindById(userId);
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

            if (string.IsNullOrWhiteSpace(request.Password) || !hashService.Verify(request.Password, user.PasswordHash))
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
                        RevealDurationSeconds = options.CardRevealDurationSeconds
                    };
                }

                if (!otpService.VerifyTOTP(user.Id, request.OtpCode))
                {
                    return new RevealCardResponse
                    {
                        Success = false,
                        Message = "Invalid or expired OTP code."
                    };
                }

                otpService.InvalidateOTP(user.Id);
            }

            return new RevealCardResponse
            {
                Success = true,
                Message = "Sensitive card details revealed successfully.",
                RevealDurationSeconds = options.CardRevealDurationSeconds,
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

                if (request.SpendingLimit.Value > options.MaximumSpendingLimit)
                {
                    return CreateCommandFailure($"Spending limit cannot exceed {options.MaximumSpendingLimit:0.##}.");
                }
            }

            decimal? spendingLimit = request.SpendingLimit ?? card.MonthlySpendingCap;
            bool isOnlineEnabled = request.IsOnlinePaymentsEnabled ?? card.IsOnlineEnabled;
            bool isContactlessEnabled = request.IsContactlessPaymentsEnabled ?? card.IsContactlessEnabled;

            bool updated = cardRepository.UpdateSettings(cardId, spendingLimit, isOnlineEnabled, isContactlessEnabled);
            if (!updated)
            {
                return CreateCommandFailure("Failed to update card settings.");
            }

            Card refreshedCard = cardRepository.GetCardById(cardId) !;
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

            bool updated = cardRepository.SaveSortPreference(userId, sortOption);
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

            bool updated = cardRepository.UpdateStatus(cardId, status);
            if (!updated)
            {
                return CreateCommandFailure("Failed to update card status.");
            }

            Card refreshedCard = cardRepository.GetCardById(cardId) !;
            return new CardCommandResponse
            {
                Success = true,
                Message = successMessage,
                Card = MapToSummary(refreshedCard)
            };
        }

        private Card? GetOwnedCard(int userId, int cardId)
        {
            Card? card = cardRepository.GetCardById(cardId);
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
            string otp = otpService.GenerateTOTP(user.Id);
            if (string.IsNullOrWhiteSpace(user.Preferred2FAMethod) ||
                string.Equals(user.Preferred2FAMethod, "Email", StringComparison.OrdinalIgnoreCase))
            {
                emailService.SendOTPCode(user.Email, otp);
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

        private static bool IsDigitsOnly(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }

        private static bool IsLuhnValid(string number)
        {
            int sum = 0;
            bool alternate = false;
            for (int i = number.Length - 1; i >= 0; i--)
            {
                char c = number[i];
                if (!char.IsDigit(c)) return false;
                int n = c - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            return (sum % 10) == 0;
        }

        private static string GenerateCardNumber(int length)
        {
            if (length < 13) length = 16;
            // generate length-1 random digits and compute Luhn check digit
            int payloadLength = length - 1;
            Span<char> digits = stackalloc char[length];
            for (int i = 0; i < payloadLength; i++)
            {
                digits[i] = (char)('0' + System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10));
            }

            // compute check digit
            int sum = 0;
            bool alternate = true; // because we'll add check digit at end
            for (int i = payloadLength - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }

            int mod = sum % 10;
            int check = mod == 0 ? 0 : 10 - mod;
            digits[payloadLength] = (char)('0' + check);

            return new string(digits);
        }

        private static string GenerateCvv(int length)
        {
            if (length < 3) length = 3;
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = (char)('0' + System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10));
            }
            return new string(buffer);
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
