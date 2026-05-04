using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Infrastructure.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace BankApp.Server.Tests
{
    [TestFixture]

    public class ProfileServiceTests
    {
        private IUserRepository mockUserRepository;
        private IHashService mockHashService;

        private ProfileService profileService;

        [SetUp]
        public void Setup()
        {
            mockUserRepository = Substitute.For<IUserRepository>();
            mockHashService = Substitute.For<IHashService>();

            profileService = new ProfileService(mockUserRepository, mockHashService);
        }

        [Test]
        public void GetUserById_UserIdNull_ReturnsNull()
        {
            User? user = profileService.GetUserById(0);
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserById_MockUser_ReturnsUser()
        {
            User mockUser = new User { Id = 1, FullName = "John Doe" };
            mockUserRepository.FindById(1).Returns(mockUser);
            User? user = profileService.GetUserById(1);
            Assert.That(user, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Equals(mockUser), Is.True);
            }
        }

        [Test]
        public void LinkOAuth_ReturnsException()
        {
            Assert.Throws<NotImplementedException>(() => profileService.LinkOAuth(1, "Google"));
        }

        [Test]
        public void UnlinkOAuth_ReturnsException()
        {
            Assert.Throws<NotImplementedException>(() => profileService.UnlinkOAuth(1, "Google"));
        }

        [Test]
        public void UpdatePersonalInfo_UserIdNull_ReturnsFailure()
        {
            UpdateProfileResponse updateProfileResponse =
                profileService.UpdatePersonalInfo(new UpdateProfileRequest());

            UpdateProfileResponse expectedResponse = new UpdateProfileResponse
            {
                Success = false,
                Message = "Something went wrong. Please try again."
            };

            Assert.That(updateProfileResponse.Equals(expectedResponse), Is.True);
        }

        [Test]
        public void UpdatePersonalInfo_UserNotFound_ReturnsFailure()
        {
            mockUserRepository.FindById(Arg.Any<int>()).Returns((User?)null);
            UpdateProfileResponse updateProfileResponse =
                profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1 });

            UpdateProfileResponse expectedResponse = new UpdateProfileResponse
            {
                Success = false,
                Message = "User not found."
            };

            Assert.That(updateProfileResponse.Equals(expectedResponse), Is.True);
        }

        [Test]
        public void UpdatePersonalInfo_InvalidPhoneNumber_ReturnsFailure()
        {
            User user = new User { Id = 1, PhoneNumber = "1234567890" };
            mockUserRepository.FindById(1).Returns(user);
            UpdateProfileResponse updateProfileResponse =
                profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "invalid-phone" });

            UpdateProfileResponse expectedResponse = new UpdateProfileResponse
            {
                Success = false,
                Message = "Invalid phone number."
            };

            Assert.That(updateProfileResponse.Equals(expectedResponse), Is.True);
        }

        [Test]
        public void UpdatePersonalInfo_UserRepositoryError_ReturnsFailure()
        {
            User user = new User { Id = 1, PhoneNumber = "1234567890", Address = "Old Address" };
            mockUserRepository.FindById(1).Returns(user);
            mockUserRepository.UpdateUser(user).Returns(false);

            UpdateProfileResponse updateProfileResponse =
                profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "0987654321", Address = "New Address" });

            UpdateProfileResponse expectedResponse = new UpdateProfileResponse
            {
                Success = false,
                Message = "Could not update user."
            };

            Assert.That(updateProfileResponse.Equals(expectedResponse), Is.True);
        }

        [Test]
        public void UpdatePersonalInfo_UserRepositoryUpdatesChanges_ReturnsSuccess()
        {
            User user = new User { Id = 1, PhoneNumber = "1234567890", Address = "Old Address" };
            mockUserRepository.FindById(1).Returns(user);
            mockUserRepository.UpdateUser(user).Returns(true);
            UpdateProfileResponse updateProfileResponse =
                profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "0987654321", Address = "New Address" });

            User updatedUser = new User { Id = 1, PhoneNumber = "0987654321", Address = "New Address" };

            Assert.That(user.Equals(updatedUser), Is.True);
        }

        [Test]
        public void UpdatePersonalInfo_ValidRequest_ReturnsSuccess()
        {
            User user = new User { Id = 1, PhoneNumber = "1234567890", Address = "Old Address" };
            mockUserRepository.FindById(1).Returns(user);
            mockUserRepository.UpdateUser(user).Returns(true);
            UpdateProfileResponse updateProfileResponse =
                profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "0987654321", Address = "New Address" });

            UpdateProfileResponse expectedResponse = new UpdateProfileResponse
            {
                Success = true,
                Message = "User profile updated successfully."
            };

            Assert.That(updateProfileResponse.Equals(expectedResponse), Is.True);
        }

        [Test]

        public void ChangePassword_UserIdNull_ReturnsFailure()
        {
            ChangePasswordResponse changePasswordResponse =
                profileService.ChangePassword(new ChangePasswordRequest());

            ChangePasswordResponse expectedResponse = new ChangePasswordResponse
            {
                Success = false,
                Message = "User not found."
            };

            Assert.That(changePasswordResponse.Equals(expectedResponse), Is.True);
        }

        [Test]

        public void ChangePassword_IncorrectCurrentPassword_ReturnsFailure()
        {
            User user = new User { Id = 1, PasswordHash = "hashed-password" };
            mockUserRepository.FindById(1).Returns(user);
            mockHashService.Verify("wrong-password", "hashed-password").Returns(false);
            ChangePasswordResponse changePasswordResponse =
                profileService.ChangePassword(new ChangePasswordRequest { UserId = 1, CurrentPassword = "wrong-password", NewPassword = "NewStrongP@ssw0rd" });

            ChangePasswordResponse expectedResponse = new ChangePasswordResponse
            {
                Success = false,
                Message = "Current password is incorrect. Please try again."
            };

            Assert.That(changePasswordResponse.Equals(expectedResponse), Is.True);
        }

        [Test]
        public void ChangePassword_CorrectCurrentPassword_ReturnsSuccess()
        {
            User user = new User { Id = 1, PasswordHash = "hashed-password" };
            mockUserRepository.FindById(1).Returns(user);
            mockHashService.Verify("correct-password", "hashed-password").Returns(true);
            mockHashService.GetHash("NewStrongP@ssw0rd").Returns("new-hashed-password");
            ChangePasswordResponse changePasswordResponse =
                profileService.ChangePassword(new ChangePasswordRequest { UserId = 1, CurrentPassword = "correct-password", NewPassword = "NewStrongP@ssw0rd" });
            ChangePasswordResponse expectedResponse = new ChangePasswordResponse
            {
                Success = true,
                Message = "Password changed successfully."
            };

            Assert.That(changePasswordResponse.Equals(expectedResponse), Is.True);
        }

        [Test]
        public void ChangePassword_CheckIfPasswordChanged_ReturnsSuccess()
        {
            User user = new User { Id = 1, PasswordHash = "hashed-password" };
            mockUserRepository.FindById(1).Returns(user);
            mockHashService.Verify("correct-password", "hashed-password").Returns(true);
            mockHashService.GetHash("NewStrongP@ssw0rd").Returns("new-hashed-password");
            ChangePasswordResponse changePasswordResponse =
                profileService.ChangePassword(new ChangePasswordRequest { UserId = 1, CurrentPassword = "correct-password", NewPassword = "NewStrongP@ssw0rd" });

            Assert.That(user.PasswordHash, Is.EqualTo("new-hashed-password"));
        }

        [Test]

        public void Enable2FA_UserIdNull_ReturnsFailure()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User?)null);
            bool twoFactorResponse = profileService.Enable2FA(1, new TwoFactorMethod());

            Assert.That(twoFactorResponse, Is.False);
        }

        [Test]

        public void Enable2FA_ValidRequest_ReturnsSuccess()
        {
            User user = CardServiceTests.CreateUser(true);
            mockUserRepository.FindById(user.Id).Returns(user);
            mockUserRepository.UpdateUser(user).Returns(true);
            TwoFactorMethod method = TwoFactorMethod.Email;
            bool twoFactorResponse = profileService.Enable2FA(user.Id, method);

            Assert.That(twoFactorResponse, Is.True);
        }

        [Test]
        public void Enable2FA_CheckIfEnabled_ReturnsSuccess()
        {
            User userWith2FA = new User { Id = 1 };
            mockUserRepository.FindById(userWith2FA.Id).Returns(userWith2FA);
            mockUserRepository.UpdateUser(userWith2FA).Returns(true);
            TwoFactorMethod method = TwoFactorMethod.Email;
            bool twoFactorResponse = profileService.Enable2FA(userWith2FA.Id, method);

            User updatedUser = new User { Id = userWith2FA.Id, Is2FAEnabled = true, Preferred2FAMethod = method.ToString() };

            Assert.That(userWith2FA.Equals(updatedUser), Is.True);
        }

        [Test]
        public void Disable2FA_UserIdNull_ReturnsFailure()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User?)null);
            bool twoFactorResponse = profileService.Disable2FA(1);

            Assert.That(twoFactorResponse, Is.False);
        }

        [Test]
        public void Disable2FA_ValidRequest_ReturnsSuccess()
        {
            User userWithout2FA = CardServiceTests.CreateUser(true);
            mockUserRepository.FindById(userWithout2FA.Id).Returns(userWithout2FA);
            mockUserRepository.UpdateUser(userWithout2FA).Returns(true);
            bool twoFactorResponse = profileService.Disable2FA(userWithout2FA.Id);

            Assert.That(twoFactorResponse, Is.True);
        }

        [Test]
        public void Disable2FA_CheckIfDisabled_ReturnsSuccess()
        {
            User userWithout2FA = new User { Id = 1 };
            mockUserRepository.FindById(userWithout2FA.Id).Returns(userWithout2FA);
            mockUserRepository.UpdateUser(userWithout2FA).Returns(true);
            bool twoFactorResponse = profileService.Disable2FA(userWithout2FA.Id);

            User updatedUser = new User { Id = userWithout2FA.Id, Is2FAEnabled = false };

            Assert.That(userWithout2FA.Equals(updatedUser), Is.True);
        }

        [Test]
        public void GetOAuthLinks_UserIdNull_ReturnsEmptyList()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User?)null);
            List<OAuthLink> links = profileService.GetOAuthLinks(1);

            Assert.That(links, Is.Empty);
        }

        [Test]
        public void GetOAuthLinks_ValidRequest_ReturnsLinks()
        {
            User user = CardServiceTests.CreateUser(true);
            List<OAuthLink> mockLinks = new List<OAuthLink>
            {
                new OAuthLink { Id = 1, Provider = "Google", ProviderUserId = "google-123" },
                new OAuthLink { Id = 2, Provider = "Facebook", ProviderUserId = "fb-456" }
            };
            mockUserRepository.FindById(user.Id).Returns(user);
            mockUserRepository.GetLinkedProviders(user.Id).Returns(mockLinks);
            List<OAuthLink> links = profileService.GetOAuthLinks(user.Id);

            Assert.That(links.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetOAuthLinks_CheckIfOAuthLinksAreCorrect_ReturnsLinks()
        {
            User user = CardServiceTests.CreateUser(true);
            List<OAuthLink> mockLinks = new List<OAuthLink>
            {
                new OAuthLink { Id = 1, Provider = "Google", ProviderUserId = "google-123" },
            };
            mockUserRepository.FindById(user.Id).Returns(user);
            mockUserRepository.GetLinkedProviders(user.Id).Returns(mockLinks);
            List<OAuthLink> links = profileService.GetOAuthLinks(user.Id);
            OAuthLink link = links[0];
            OAuthLink expectedLink = new OAuthLink { Id = 1, Provider = "Google", ProviderUserId = "google-123" };

            Assert.That(link.Equals(expectedLink), Is.True);
        }

        [Test]
        public void GetNotificationPreferences_UserIdNull_ReturnsEmptyList()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User?)null);
            List<NotificationPreference> prefs = profileService.GetNotificationPreferences(1);

            Assert.That(prefs, Is.Empty);
        }

        [Test]
        public void GetNotificationPreferences_ValidRequest_ReturnsPreferences()
        {
            User user = CardServiceTests.CreateUser(true);
            List<NotificationPreference> mockPrefs = new List<NotificationPreference>
            {
                new NotificationPreference { Id = 1, EmailEnabled = true },
                new NotificationPreference { Id = 2, SmsEnabled = false }
            };
            mockUserRepository.FindById(user.Id).Returns(user);
            mockUserRepository.GetNotificationPreferences(user.Id).Returns(mockPrefs);
            List<NotificationPreference> prefs = profileService.GetNotificationPreferences(user.Id);

            Assert.That(prefs.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetNotificationPreferences_CheckIfPreferencesAreCorrect_ReturnsPreferences()
        {
            User user = CardServiceTests.CreateUser(true);
            List<NotificationPreference> mockPrefs = new List<NotificationPreference>
            {
                new NotificationPreference { Id = 1, EmailEnabled = true },
            };
            mockUserRepository.FindById(user.Id).Returns(user);
            mockUserRepository.GetNotificationPreferences(user.Id).Returns(mockPrefs);
            List<NotificationPreference> prefs = profileService.GetNotificationPreferences(user.Id);
            NotificationPreference preference = prefs[0];
            NotificationPreference expectedPref = new NotificationPreference { Id = 1, EmailEnabled = true };

            Assert.That(preference.Equals(expectedPref), Is.True);
        }

        [Test]
        public void UpdateNotificationPreferences_UserIdNull_ReturnsFailure()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User?)null);
            bool result = profileService.UpdateNotificationPreferences(1, new List<NotificationPreference>());

            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateNotificationPreferences_ValidRequest_ReturnsSuccess()
        {
            User user = CardServiceTests.CreateUser(true);
            List<NotificationPreference> newPrefs = new List<NotificationPreference>
            {
                new NotificationPreference { Id = 1, EmailEnabled = false },
                new NotificationPreference { Id = 2, SmsEnabled = true }
            };
            mockUserRepository.FindById(user.Id).Returns(user);
            mockUserRepository.UpdateNotificationPreferences(user.Id, newPrefs).Returns(true);
            bool result = profileService.UpdateNotificationPreferences(user.Id, newPrefs);

            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_UserIdNull_ReturnsFalse()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User?)null);
            bool result = profileService.VerifyPassword(1, "any-password");

            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyPassword_UserFound_ReturnsHashVerificationResult()
        {
            User user = new User { Id = 1, PasswordHash = "hashed-password" };
            mockUserRepository.FindById(1).Returns(user);
            mockHashService.Verify("input-password", "hashed-password").Returns(true);
            bool result = profileService.VerifyPassword(1, "input-password");

            Assert.That(result, Is.True);
        }
    }
}
