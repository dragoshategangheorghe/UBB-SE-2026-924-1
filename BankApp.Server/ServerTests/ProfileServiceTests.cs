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
        private IUserRepository _mockUserRepository;
        private IHashService _mockHashService;

        private ProfileService _profileService;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = Substitute.For<IUserRepository>();
            _mockHashService = Substitute.For<IHashService>();

            _profileService = new ProfileService(_mockUserRepository, _mockHashService);
        }

        [Test]
        public void GetUserById_UserIdNull_ReturnsNull()
        {
            User? user = _profileService.GetUserById(0);
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserById_MockUser_ReturnsUser()
        {
            User mockUser = new User { Id = 1, FullName = "John Doe" };
            _mockUserRepository.FindById(1).Returns(mockUser);
            User? user = _profileService.GetUserById(1);
            Assert.That(user, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Equals(mockUser), Is.True);
            }
        }

        [Test]
        public void LinkOAuth_ReturnsException()
        {
            Assert.Throws<NotImplementedException>(() => _profileService.LinkOAuth(1, "Google"));
        }

        [Test]
        public void UnlinkOAuth_ReturnsException()
        {
            Assert.Throws<NotImplementedException>(() => _profileService.UnlinkOAuth(1, "Google"));
        }

        [Test]
        public void UpdatePersonalInfo_UserIdNull_ReturnsFailure()
        {
            UpdateProfileResponse updateProfileResponse =
                _profileService.UpdatePersonalInfo(new UpdateProfileRequest());

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
            _mockUserRepository.FindById(Arg.Any<int>()).Returns((User?)null);
            UpdateProfileResponse updateProfileResponse =
                _profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1 });

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
            _mockUserRepository.FindById(1).Returns(user);
            UpdateProfileResponse updateProfileResponse =
                _profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "invalid-phone" });

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
            _mockUserRepository.FindById(1).Returns(user);
            _mockUserRepository.UpdateUser(user).Returns(false);

            UpdateProfileResponse updateProfileResponse =
                _profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "0987654321", Address = "New Address" });

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
            _mockUserRepository.FindById(1).Returns(user);
            _mockUserRepository.UpdateUser(user).Returns(true);
            UpdateProfileResponse updateProfileResponse =
                _profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "0987654321", Address = "New Address" });

            User updatedUser = new User { Id = 1, PhoneNumber = "0987654321", Address = "New Address" };

            Assert.That(user.Equals(updatedUser), Is.True);
        }

        [Test]
        public void UpdatePersonalInfo_ValidRequest_ReturnsSuccess()
        {
            User user = new User { Id = 1, PhoneNumber = "1234567890", Address = "Old Address" };
            _mockUserRepository.FindById(1).Returns(user);
            _mockUserRepository.UpdateUser(user).Returns(true);
            UpdateProfileResponse updateProfileResponse =
                _profileService.UpdatePersonalInfo(new UpdateProfileRequest { UserId = 1, PhoneNumber = "0987654321", Address = "New Address" });

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
                _profileService.ChangePassword(new ChangePasswordRequest());

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
            _mockUserRepository.FindById(1).Returns(user);
            _mockHashService.Verify("wrong-password", "hashed-password").Returns(false);
            ChangePasswordResponse changePasswordResponse =
                _profileService.ChangePassword(new ChangePasswordRequest { UserId = 1, CurrentPassword = "wrong-password", NewPassword = "NewStrongP@ssw0rd" });

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
            _mockUserRepository.FindById(1).Returns(user);
            _mockHashService.Verify("correct-password", "hashed-password").Returns(true);
            _mockHashService.GetHash("NewStrongP@ssw0rd").Returns("new-hashed-password");
            ChangePasswordResponse changePasswordResponse =
                _profileService.ChangePassword(new ChangePasswordRequest { UserId = 1, CurrentPassword = "correct-password", NewPassword = "NewStrongP@ssw0rd" });
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
            _mockUserRepository.FindById(1).Returns(user);
            _mockHashService.Verify("correct-password", "hashed-password").Returns(true);
            _mockHashService.GetHash("NewStrongP@ssw0rd").Returns("new-hashed-password");
            ChangePasswordResponse changePasswordResponse =
                _profileService.ChangePassword(new ChangePasswordRequest { UserId = 1, CurrentPassword = "correct-password", NewPassword = "NewStrongP@ssw0rd" });

            Assert.That(user.PasswordHash, Is.EqualTo("new-hashed-password"));
        }

        [Test]

        public void Enable2FA_UserIdNull_ReturnsFailure()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User?)null);
            bool twoFactorResponse = _profileService.Enable2FA(1, new TwoFactorMethod());

            Assert.That(twoFactorResponse, Is.False);
        }

        [Test]

        public void Enable2FA_ValidRequest_ReturnsSuccess()
        {
            User user = CardServiceTests.CreateUser(true);
            _mockUserRepository.FindById(user.Id).Returns(user);
            _mockUserRepository.UpdateUser(user).Returns(true);
            TwoFactorMethod method = TwoFactorMethod.Email;
            bool twoFactorResponse = _profileService.Enable2FA(user.Id, method);

            Assert.That(twoFactorResponse, Is.True);
        }

        [Test]
        public void Enable2FA_CheckIfEnabled_ReturnsSuccess()
        {
            User userWith2FA = new User { Id = 1 };
            _mockUserRepository.FindById(userWith2FA.Id).Returns(userWith2FA);
            _mockUserRepository.UpdateUser(userWith2FA).Returns(true);
            TwoFactorMethod method = TwoFactorMethod.Email;
            bool twoFactorResponse = _profileService.Enable2FA(userWith2FA.Id, method);

            User updatedUser = new User { Id = userWith2FA.Id, Is2FAEnabled = true, Preferred2FAMethod = method.ToString() };

            Assert.That(userWith2FA.Equals(updatedUser), Is.True);
        }

        [Test]
        public void Disable2FA_UserIdNull_ReturnsFailure()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User?)null);
            bool twoFactorResponse = _profileService.Disable2FA(1);

            Assert.That(twoFactorResponse, Is.False);
        }

        [Test]
        public void Disable2FA_ValidRequest_ReturnsSuccess()
        {
            User userWithout2FA = CardServiceTests.CreateUser(true);
            _mockUserRepository.FindById(userWithout2FA.Id).Returns(userWithout2FA);
            _mockUserRepository.UpdateUser(userWithout2FA).Returns(true);
            bool twoFactorResponse = _profileService.Disable2FA(userWithout2FA.Id);

            Assert.That(twoFactorResponse, Is.True);
        }

        [Test]
        public void Disable2FA_CheckIfDisabled_ReturnsSuccess()
        {
            User userWithout2FA = new User { Id = 1 };
            _mockUserRepository.FindById(userWithout2FA.Id).Returns(userWithout2FA);
            _mockUserRepository.UpdateUser(userWithout2FA).Returns(true);
            bool twoFactorResponse = _profileService.Disable2FA(userWithout2FA.Id);

            User updatedUser = new User { Id = userWithout2FA.Id, Is2FAEnabled = false };

            Assert.That(userWithout2FA.Equals(updatedUser), Is.True);
        }

        [Test]
        public void GetOAuthLinks_UserIdNull_ReturnsEmptyList()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User?)null);
            List<OAuthLink> links = _profileService.GetOAuthLinks(1);

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
            _mockUserRepository.FindById(user.Id).Returns(user);
            _mockUserRepository.GetLinkedProviders(user.Id).Returns(mockLinks);
            List<OAuthLink> links = _profileService.GetOAuthLinks(user.Id);

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
            _mockUserRepository.FindById(user.Id).Returns(user);
            _mockUserRepository.GetLinkedProviders(user.Id).Returns(mockLinks);
            List<OAuthLink> links = _profileService.GetOAuthLinks(user.Id);
            OAuthLink link = links[0];
            OAuthLink expectedLink = new OAuthLink { Id = 1, Provider = "Google", ProviderUserId = "google-123" };

            Assert.That(link.Equals(expectedLink), Is.True);
        }

        [Test]
        public void GetNotificationPreferences_UserIdNull_ReturnsEmptyList()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User?)null);
            List<NotificationPreference> prefs = _profileService.GetNotificationPreferences(1);

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
            _mockUserRepository.FindById(user.Id).Returns(user);
            _mockUserRepository.GetNotificationPreferences(user.Id).Returns(mockPrefs);
            List<NotificationPreference> prefs = _profileService.GetNotificationPreferences(user.Id);

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
            _mockUserRepository.FindById(user.Id).Returns(user);
            _mockUserRepository.GetNotificationPreferences(user.Id).Returns(mockPrefs);
            List<NotificationPreference> prefs = _profileService.GetNotificationPreferences(user.Id);
            NotificationPreference preference = prefs[0];
            NotificationPreference expectedPref = new NotificationPreference { Id = 1, EmailEnabled = true };

            Assert.That(preference.Equals(expectedPref), Is.True);
        }

        [Test]
        public void UpdateNotificationPreferences_UserIdNull_ReturnsFailure()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User?)null);
            bool result = _profileService.UpdateNotificationPreferences(1, new List<NotificationPreference>());

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
            _mockUserRepository.FindById(user.Id).Returns(user);
            _mockUserRepository.UpdateNotificationPreferences(user.Id, newPrefs).Returns(true);
            bool result = _profileService.UpdateNotificationPreferences(user.Id, newPrefs);

            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_UserIdNull_ReturnsFalse()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User?)null);
            bool result = _profileService.VerifyPassword(1, "any-password");

            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyPassword_UserFound_ReturnsHashVerificationResult()
        {
            User user = new User { Id = 1, PasswordHash = "hashed-password" };
            _mockUserRepository.FindById(1).Returns(user);
            _mockHashService.Verify("input-password", "hashed-password").Returns(true);
            bool result = _profileService.VerifyPassword(1, "input-password");

            Assert.That(result, Is.True);
        }
    }
}
