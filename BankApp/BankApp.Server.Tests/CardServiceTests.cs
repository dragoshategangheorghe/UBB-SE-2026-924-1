using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Cards;
using BankApp.Models.DTOs.Dashboard;
using BankApp.Models.Entities;
using BankApp.Server.Configuration;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class CardServiceTests
    {
        private ICardRepository mockCardRepository;
        private IUserRepository mockUserRepository;
        private IHashService mockHashService;
        private IOTPService mockOtpService;
        private IEmailService mockEmailService;

        private CardService cardService;

        [SetUp]
        public void SetUp()
        {
             mockCardRepository = Substitute.For<ICardRepository>();
             mockUserRepository = Substitute.For<IUserRepository>();
             mockHashService = Substitute.For<IHashService>();
             mockOtpService = Substitute.For<IOTPService>();
             mockEmailService = Substitute.For<IEmailService>();

             cardService = new CardService(
                 mockCardRepository,
                 mockUserRepository,
                 mockHashService,
                 mockOtpService,
                 mockEmailService,
                 Options.Create(new TeamCOptions()));
        }

        [Test]
        public void UpdateSettings_CardDoesNotExist_ReturnsFailure()
        {
            int cardId = 1;
            mockCardRepository.GetCardById(cardId).Returns((Card)null!);
            CardCommandResponse cardCommandResponse =
                cardService.UpdateSettings(2, cardId, new UpdateCardSettingsRequest());

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card not found."));
            }
        }

        [Test]
        public void UpdateSettings_CardSettingsFailToUpdateInCardRepository_ReturnsFailure()
        {
            Card card = CreateCard();
            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockCardRepository
                .UpdateSettings(card.Id, card.MonthlySpendingCap, card.IsOnlineEnabled, card.IsContactlessEnabled)
                .Returns(false);

            CardCommandResponse cardCommandResponse =
                cardService.UpdateSettings(card.UserId, card.Id, new UpdateCardSettingsRequest());

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Failed to update card settings."));
            }
        }

        [Test]
        public void UpdateSettings_CardExistsAndRepositoryUpdates_ReturnsSuccess()
        {
            Card card = CreateCard();
            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockCardRepository
                .UpdateSettings(card.Id, 1000m, true, true)
                .Returns(true);

            CardCommandResponse cardCommandResponse =
                cardService.UpdateSettings(card.UserId, card.Id, new UpdateCardSettingsRequest
                {
                SpendingLimit = 1000m,
                IsOnlinePaymentsEnabled = true,
                IsContactlessPaymentsEnabled = true
            });

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.True);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card settings updated successfully."));
            }
        }

        [Test]
        public void UpdateSortPreference_InvalidSortOption_ReturnsFailure()
        {
            User user = CreateUser(false);
            UpdateCardSortPreferenceRequest sortRequest = new UpdateCardSortPreferenceRequest { SortOption = "wrong" };

            CardCommandResponse cardCommandResponse = cardService.UpdateSortPreference(user.Id, sortRequest);

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Unsupported card sort option."));
            }
        }
        [Test]
        public void UpdateSortPreference_SortOptionsFailToUpdateInCardRepositor_ReturnsFailure()
        {
            User user = CreateUser(false);
            string validSortOption = CardSortOptions.ExpiryDate;
            UpdateCardSortPreferenceRequest sortRequest = new UpdateCardSortPreferenceRequest { SortOption = validSortOption };

            mockCardRepository.SaveSortPreference(user.Id, validSortOption).Returns(false);

            CardCommandResponse cardCommandResponse = cardService.UpdateSortPreference(user.Id, sortRequest);

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Failed to update card sort preference."));
            }
        }
        [Test]
        public void UpdateSortPreference_ValidSortOptionAndRepositoryUpdates_ReturnsSuccess()
        {
            User user = CreateUser(false);
            string validSortOption = CardSortOptions.ExpiryDate;
            UpdateCardSortPreferenceRequest sortRequest = new UpdateCardSortPreferenceRequest { SortOption = validSortOption };

            mockCardRepository.SaveSortPreference(user.Id, validSortOption).Returns(true);

            CardCommandResponse cardCommandResponse = cardService.UpdateSortPreference(user.Id, sortRequest);

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.True);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card sort preference updated successfully."));
            }
        }

        [Test]
        public void FreezeCard_OwnedUnfrozenCardExists_UpdatesStatus()
        {
            Card activeCard = CreateCard();
            Card frozenCard = CreateCard(); // same data! Only status is changed
            frozenCard.Status = "Frozen";

            mockCardRepository.GetCardById(activeCard.Id).Returns(activeCard, frozenCard); // returns active then on the next call frozen
            mockCardRepository.UpdateStatus(activeCard.Id, "Frozen").Returns(true);

            CardCommandResponse cardCommandResponse = cardService.FreezeCard(activeCard.UserId, activeCard.Id);
            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.True);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card frozen successfully."));
                Assert.That(cardCommandResponse.Card?.Status, Is.EqualTo("Frozen"));
            }
        }

        [Test]
        public void UnfreezeCard_OwnedUnfrozenCardExists_UpdatesStatus()
        {
            Card activeCard = CreateCard();

            mockCardRepository.GetCardById(activeCard.Id).Returns(activeCard, activeCard); // returns active then on the next call frozen
            mockCardRepository.UpdateStatus(activeCard.Id, "Active").Returns(true);

            CardCommandResponse cardCommandResponse = cardService.UnfreezeCard(activeCard.UserId, activeCard.Id);
            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.True);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card unfrozen successfully."));
                Assert.That(cardCommandResponse.Card?.Status, Is.EqualTo("Active"));
            }
        }
        [Test]
        public void FreezeCard_CardDoesNotExist_ReturnsFailure()
        {
            Card card = CreateCard();

            mockCardRepository.GetCardById(card.Id).Returns((Card)null!);

            CardCommandResponse cardCommandResponse = cardService.FreezeCard(card.UserId, card.Id);
            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card not found."));
            }
        }

        [Test]
        public void UnreezeCard_CardDoesNotExist_ReturnsFailure()
        {
            Card card = CreateCard();

            mockCardRepository.GetCardById(card.Id).Returns((Card)null!);

            CardCommandResponse cardCommandResponse = cardService.UnfreezeCard(card.UserId, card.Id);
            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Card not found."));
            }
        }
        [Test]
        public void FreezeCard_RepositoryFailsToUpdate_ReturnsFailure()
        {
            Card card = CreateCard();

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockCardRepository.UpdateStatus(card.Id, "Frozen").Returns(false);

            CardCommandResponse cardCommandResponse = cardService.FreezeCard(card.UserId, card.Id);
            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Failed to update card status."));
            }
        }
        [Test]
        public void UnfreezeCard_RepositoryFailsToUpdate_ReturnsFailure()
        {
            Card card = CreateCard();
            card.Status = "Frozen";

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockCardRepository.UpdateStatus(card.Id, "Active").Returns(false);

            CardCommandResponse cardCommandResponse = cardService.UnfreezeCard(card.UserId, card.Id);
            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Is.EqualTo("Failed to update card status."));
            }
        }

        [Test]
        public void RevealSensitiveDetails_TwoFactorEnabledAndOtpDoesNotMatch_ReturnsSensitiveDetails()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.VerifyTOTP(user.Id, "67420").Returns(false);

            RevealCardResponse revealCardResponse =
            cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "secret",
                OtpCode = "654321"
            });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.False);
                Assert.That(revealCardResponse.Message, Is.EqualTo("Invalid or expired OTP code."));
            }
        }

        [Test]
        public void RevealSensitiveDetails_UserDoesNotExist_ReturnsSensitiveDetails()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns((User)null!);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.VerifyTOTP(user.Id, "67420").Returns(false);

            RevealCardResponse revealCardResponse =
                cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
                {
                    Password = "secret",
                    OtpCode = "654321"
                });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.False);
                Assert.That(revealCardResponse.Message, Is.EqualTo("User not found."));
            }
        }

        [Test]
        public void RevealSensitiveDetails_CardDoesNotExist_ReturnsSensitiveDetails()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns((Card)null!);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.VerifyTOTP(user.Id, "67420").Returns(false);

            RevealCardResponse revealCardResponse =
                cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
                {
                    Password = "secret",
                    OtpCode = "654321"
                });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.False);
                Assert.That(revealCardResponse.Message, Is.EqualTo("Card not found."));
            }
        }
        [Test]
        public void RevealSensitiveDetails_PasswordMatchesAndTwoFactorDisabled_ReturnsSensitiveDetails()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: false);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "secret"
            });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.True);
                Assert.That(revealCardResponse.SensitiveDetails, Is.Not.Null);
                Assert.That(revealCardResponse.SensitiveDetails!.CardNumber, Is.EqualTo(card.CardNumber));
                Assert.That(revealCardResponse.SensitiveDetails!.Cvv, Is.EqualTo(card.CVV));
            }
        }

        [Test]
        public void RevealSensitiveDetails_TwoFactorEnabledAndOtpMissing_RequiresOtp()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.GenerateTOTP(user.Id).Returns("123456");

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "secret"
            });
            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.False);
                Assert.That(revealCardResponse.RequiresOtp, Is.True);
            }
        }

        [Test]
        public void RevealSensitiveDetails_TwoFactorEnabledAndOtpMissing_SendRequiredOtpToUser()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.GenerateTOTP(user.Id).Returns("123456");

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "secret"
            });

            mockEmailService.Received(1).SendOTPCode(user.Email, "123456"); // acts as an assert !!
        }

        [Test]
        public void RevealSensitiveDetails_PasswordDoesNotMatch_ReturnsFailure()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: false);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("wrong-secret", user.PasswordHash).Returns(false);

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "wrong-secret"
            });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.False);
                Assert.That(revealCardResponse.RequiresOtp, Is.False);
                Assert.That(revealCardResponse.SensitiveDetails, Is.Null);
                Assert.That(revealCardResponse.Message, Does.Contain("Password verification failed"));
            }
        }

        [Test]
        public void RevealSensitiveDetails_PasswordDoesNotMatch_OTPCodeFunctionsAreNeverCalled()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: false);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("wrong-secret", user.PasswordHash).Returns(false);

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "wrong-secret"
            });

            // these were NEVER called, we know it returns, and they are never called; but I think it's useful to test it and I also practice NSubstitute
            mockOtpService.Received(0).VerifyTOTP(Arg.Any<int>(), Arg.Any<string>());
            mockEmailService.Received(0).SendOTPCode(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void RevealSensitiveDetails_TwoFactorEnabledAndOtpMatches_ReturnsSensitiveDetails()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.VerifyTOTP(user.Id, "654321").Returns(true);

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "secret",
                OtpCode = "654321"
            });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.True);
                Assert.That(revealCardResponse.RequiresOtp, Is.False);
                Assert.That(revealCardResponse.SensitiveDetails, Is.Not.Null);
                Assert.That(revealCardResponse.SensitiveDetails.CardNumber, Is.EqualTo(card.CardNumber));
                Assert.That(revealCardResponse.SensitiveDetails.Cvv, Is.EqualTo(card.CVV));
                Assert.That(revealCardResponse.Message, Is.EqualTo("Sensitive card details revealed successfully."));
            }
        }

        [Test]
        public void RevealSensitiveDetails_TwoFactorEnabledAndOtpMatches_VerifiesThenInvalidatesOTP()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: true);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockHashService.Verify("secret", user.PasswordHash).Returns(true);
            mockOtpService.VerifyTOTP(user.Id, "654321").Returns(true);

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "secret",
                OtpCode = "654321"
            });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.True);
                Assert.That(revealCardResponse.RequiresOtp, Is.False);
                Assert.That(revealCardResponse.SensitiveDetails, Is.Not.Null);
                Assert.That(revealCardResponse.SensitiveDetails.CardNumber, Is.EqualTo(card.CardNumber));
                Assert.That(revealCardResponse.SensitiveDetails.Cvv, Is.EqualTo(card.CVV));
                Assert.That(revealCardResponse.Message, Is.EqualTo("Sensitive card details revealed successfully."));
            }

            mockOtpService.Received(1).VerifyTOTP(user.Id, "654321");
            mockOtpService.Received(1).InvalidateOTP(user.Id);
            mockEmailService.Received(0).SendOTPCode(Arg.Any<string>(), Arg.Any<string>()); // never sends another one
        }

        [Test]
        public void FreezeCard_OwnedFrozenCardExists_UpdatesStatus()
        {
            Card frozenCard = CreateCard();
            frozenCard.Status = "Frozen";

            mockCardRepository.GetCardById(frozenCard.Id).Returns(frozenCard, frozenCard);
            mockCardRepository.UpdateStatus(frozenCard.Id, "Frozen").Returns(true);

            CardCommandResponse cardCommandResponse = cardService.FreezeCard(frozenCard.UserId, frozenCard.Id);

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
            Assert.That(cardCommandResponse.Success, Is.True);
            Assert.That(cardCommandResponse.Card!.Status, Is.EqualTo("Frozen"));
            }
        }

        [Test]
        public void UnfreezeCard_OwnedFrozenCardExists_UpdatesStatus()
        {
            Card frozenCard = CreateCard();
            frozenCard.Status = "Frozen";
            Card activeCard = CreateCard(); // same card but Active = !Frozen

            mockCardRepository.GetCardById(frozenCard.Id).Returns(frozenCard, activeCard);
            mockCardRepository.UpdateStatus(frozenCard.Id, "Active").Returns(true);

            CardCommandResponse cardCommandResponse = cardService.UnfreezeCard(frozenCard.UserId, frozenCard.Id);

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.True);
                Assert.That(cardCommandResponse.Card!.Status, Is.EqualTo("Active"));
            }
        }

        [Test]
        public void UpdateSettings_SpendingLimitIsNegative_ReturnsFailure()
        {
            Card card = CreateCard();
            mockCardRepository.GetCardById(card.Id).Returns(card);

            CardCommandResponse cardCommandResponse = cardService.UpdateSettings(card.UserId, card.Id, new UpdateCardSettingsRequest
            {
                SpendingLimit = -5m
            });

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Does.Contain("non-negative"));
            }
        }

        [Test]
        public void UpdateSettings_SpendingLimitExceedsConfiguredMaximum_ReturnsFailure()
        {
            Card card = CreateCard();
            mockCardRepository.GetCardById(card.Id).Returns(card);

            CardService testCardServiceForExceedingLimit = new CardService(mockCardRepository, mockUserRepository,
                mockHashService, mockOtpService, mockEmailService, Options.Create(new TeamCOptions
                {
                    MaximumSpendingLimit = 1000m
                }));

            CardCommandResponse cardCommandResponse = testCardServiceForExceedingLimit.UpdateSettings(card.UserId, card.Id, new UpdateCardSettingsRequest
            {
                SpendingLimit = 1500m
            });

            Assert.That(cardCommandResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardCommandResponse.Success, Is.False);
                Assert.That(cardCommandResponse.Message, Does.Contain("cannot exceed 1000"));
            }

            mockCardRepository.Received(0)
                .UpdateSettings(Arg.Any<int>(), Arg.Any<decimal?>(), Arg.Any<bool>(), Arg.Any<bool>());
        }

        [Test]
        public void UpdateSettings_SpendingLimitExceedsConfiguredMaximum_NeverUpdatesCard()
        {
            Card card = CreateCard();
            mockCardRepository.GetCardById(card.Id).Returns(card);

            CardService testCardServiceForExceedingLimit = new CardService(mockCardRepository, mockUserRepository,
                mockHashService, mockOtpService, mockEmailService, Options.Create(new TeamCOptions
                {
                    MaximumSpendingLimit = 1000m
                }));

            CardCommandResponse cardCommandResponse = testCardServiceForExceedingLimit.UpdateSettings(card.UserId, card.Id, new UpdateCardSettingsRequest
            {
                SpendingLimit = 1500m
            });

            mockCardRepository.Received(0)
                .UpdateSettings(Arg.Any<int>(), Arg.Any<decimal?>(), Arg.Any<bool>(), Arg.Any<bool>());
        }

        [Test]
        public void GetCard_CardDoesNotExist_ReturnsFailure()
        {
            Card card = CreateCard();
            User user = CreateUser(false);
            mockCardRepository.GetCardById(card.Id).Returns((Card)null!);

            CardDetailsResponse cardDetailsResponse = cardService.GetCard(user.Id, card.Id);

            Assert.That(cardDetailsResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardDetailsResponse.Success, Is.False);
                Assert.That(cardDetailsResponse.Message, Is.EqualTo("Card not found."));
            }
        }
        [Test]
        public void GetCard_CardExists_ReturnsCardDetails()
        {
            Card card = CreateCard();
            User user = CreateUser(false);
            mockCardRepository.GetCardById(card.Id).Returns(card);

            CardDetailsResponse cardDetailsResponse = cardService.GetCard(user.Id, card.Id);

            Assert.That(cardDetailsResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cardDetailsResponse.Success, Is.True);
                Assert.That(cardDetailsResponse.Message, Is.EqualTo("Card loaded successfully."));
            }
        }
        [Test]
        public void RevealSensitiveDetails_PasswordIsEmpty_ReturnsFailure()
        {
            Card card = CreateCard();
            User user = CreateUser(isTwoFactorEnabled: false);

            mockCardRepository.GetCardById(card.Id).Returns(card);
            mockUserRepository.FindById(user.Id).Returns(user);

            RevealCardResponse revealCardResponse = cardService.RevealSensitiveDetails(user.Id, card.Id, new RevealCardRequest
            {
                Password = "   "
            });

            Assert.That(revealCardResponse, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(revealCardResponse.Success, Is.False);
                Assert.That(revealCardResponse.Message, Is.EqualTo("Password verification failed."));
            }
        }

        public static Card CreateCard()
        {
            return new Card
            {
                Id = 7,
                UserId = 3,
                AccountId = 11,
                CardNumber = "5555444433331111",
                CardholderName = "Ada Lovelace",
                ExpiryDate = new DateTime(2030, 12, 1),
                CVV = "123",
                CardType = "Debit",
                CardBrand = "Mastercard",
                Status = "Active",
                MonthlySpendingCap = 2500m,
                IsOnlineEnabled = true,
                IsContactlessEnabled = true
            };
        }

        public static User CreateUser(bool isTwoFactorEnabled)
        {
            return new User
            {
                Id = 3,
                Email = "ada@example.com",
                PasswordHash = "hashed",
                FullName = "Ada Lovelace",
                Is2FAEnabled = isTwoFactorEnabled,
                Preferred2FAMethod = "Email"
            };
        }
    }
}
