using System;
using System.Collections.Generic;
using System.Linq;
using BankApp.Models.DTOs.Cards;
using BankApp.Models.Entities;
using BankApp.Server.Configuration;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private const string ActiveCardStatus = "Active";
        private const string FrozenCardStatus = "Frozen";

        private readonly ICardRepository cardRepository;
        private readonly IUserRepository userRepository;
        private readonly IHashService hashService;
        private readonly IOTPService otpService;
        private readonly IEmailService emailService;
        private readonly TeamCOptions options;

        public CardsController(
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

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        [HttpGet]
        public IActionResult GetCards()
        {
            int userId = GetAuthenticatedUserId();
            List<Card> cards = cardRepository.GetCardsByUserId(userId);
            string sortOption = NormalizeSortOption(cardRepository.GetSortPreference(userId)?.SortOption);

            return Ok(new GetCardsResponse
            {
                Success = true,
                Message = "Cards loaded successfully.",
                SortOption = sortOption,
                Cards = SortCards(cards, sortOption).Select(MapToSummary).ToList()
            });
        }

        [HttpGet("{cardId:int}")]
        public IActionResult GetCard(int cardId)
        {
            int userId = GetAuthenticatedUserId();
            Card? card = GetOwnedCard(userId, cardId);
            CardDetailsResponse response = card == null
                ? new CardDetailsResponse { Success = false, Message = "Card not found." }
                : new CardDetailsResponse { Success = true, Message = "Card loaded successfully.", Card = MapToSummary(card) };
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost("{cardId:int}/reveal")]
        public IActionResult RevealCard(int cardId, [FromBody] RevealCardRequest request)
        {
            int userId = GetAuthenticatedUserId();
            User? user = userRepository.FindById(userId);
            Card? card = GetOwnedCard(userId, cardId);

            if (user == null)
            {
                return BadRequest(new RevealCardResponse { Success = false, Message = "User not found." });
            }

            if (card == null)
            {
                return NotFound(new RevealCardResponse { Success = false, Message = "Card not found." });
            }

            if (string.IsNullOrWhiteSpace(request.Password) || !hashService.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest(new RevealCardResponse { Success = false, Message = "Password verification failed." });
            }

            if (user.Is2FAEnabled)
            {
                if (string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    SendRevealOtp(user);
                    return Ok(new RevealCardResponse
                    {
                        Success = false,
                        RequiresOtp = true,
                        Message = "OTP verification is required before revealing card details.",
                        RevealDurationSeconds = options.CardRevealDurationSeconds
                    });
                }

                if (!otpService.VerifyTOTP(user.Id, request.OtpCode))
                {
                    return BadRequest(new RevealCardResponse { Success = false, Message = "Invalid or expired OTP code." });
                }

                otpService.InvalidateOTP(user.Id);
            }

            RevealCardResponse response = new RevealCardResponse
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
            return response.Success || response.RequiresOtp ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{cardId:int}/freeze")]
        public IActionResult FreezeCard(int cardId)
        {
            CardCommandResponse response = ChangeCardStatus(GetAuthenticatedUserId(), cardId, FrozenCardStatus, "Card frozen successfully.");
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{cardId:int}/unfreeze")]
        public IActionResult UnfreezeCard(int cardId)
        {
            CardCommandResponse response = ChangeCardStatus(GetAuthenticatedUserId(), cardId, ActiveCardStatus, "Card unfrozen successfully.");
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{cardId:int}/settings")]
        public IActionResult UpdateSettings(int cardId, [FromBody] UpdateCardSettingsRequest request)
        {
            int userId = GetAuthenticatedUserId();
            Card? card = GetOwnedCard(userId, cardId);
            if (card == null)
            {
                return BadRequest(CreateCommandFailure("Card not found."));
            }

            if (request.SpendingLimit.HasValue)
            {
                if (request.SpendingLimit.Value < 0)
                {
                    return BadRequest(CreateCommandFailure("Spending limit must be a non-negative value."));
                }

                if (request.SpendingLimit.Value > options.MaximumSpendingLimit)
                {
                    return BadRequest(CreateCommandFailure($"Spending limit cannot exceed {options.MaximumSpendingLimit:0.##}."));
                }
            }

            decimal? spendingLimit = request.SpendingLimit ?? card.MonthlySpendingCap;
            bool isOnlineEnabled = request.IsOnlinePaymentsEnabled ?? card.IsOnlineEnabled;
            bool isContactlessEnabled = request.IsContactlessPaymentsEnabled ?? card.IsContactlessEnabled;

            bool updated = cardRepository.UpdateSettings(cardId, spendingLimit, isOnlineEnabled, isContactlessEnabled);
            if (!updated)
            {
                return BadRequest(CreateCommandFailure("Failed to update card settings."));
            }

            Card refreshedCard = cardRepository.GetCardById(cardId) ??
                                 throw new InvalidOperationException("Card was not found after update.");
            CardCommandResponse response = new CardCommandResponse
            {
                Success = true,
                Message = "Card settings updated successfully.",
                Card = MapToSummary(refreshedCard)
            };
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("preferences/sort")]
        public IActionResult UpdateSortPreference([FromBody] UpdateCardSortPreferenceRequest request)
        {
            int userId = GetAuthenticatedUserId();
            string sortOption = NormalizeSortOption(request.SortOption);
            if (!IsValidSortOption(sortOption))
            {
                return BadRequest(CreateCommandFailure("Unsupported card sort option."));
            }

            bool updated = cardRepository.SaveSortPreference(userId, sortOption);
            if (!updated)
            {
                return BadRequest(CreateCommandFailure("Failed to update card sort preference."));
            }

            CardCommandResponse response = new CardCommandResponse
            {
                Success = true,
                Message = "Card sort preference updated successfully."
            };
            return response.Success ? Ok(response) : BadRequest(response);
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

            Card refreshedCard = cardRepository.GetCardById(cardId) ??
                                 throw new InvalidOperationException("Card was not found after update.");
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
