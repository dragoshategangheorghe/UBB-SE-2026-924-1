using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Moq;
using NUnit.Framework;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Entities;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> authRepoMock;
        private Mock<IHashService> hashMock;
        private Mock<IJWTService> jwtMock;
        private Mock<IOTPService> otpMock;
        private Mock<IEmailService> emailMock;

        private AuthService service;

        [SetUp]
        public void SetUp()
        {
            authRepoMock = new Mock<IAuthRepository>();
            hashMock = new Mock<IHashService>();
            jwtMock = new Mock<IJWTService>();
            otpMock = new Mock<IOTPService>();
            emailMock = new Mock<IEmailService>();

            service = new AuthService(
                authRepoMock.Object,
                hashMock.Object,
                jwtMock.Object,
                otpMock.Object,
                emailMock.Object);
        }

        private static User CreateUser(bool isTwoFactorEnabled = false, bool isLocked = false, int failedLoginAttempts = 0)
        {
            return new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = "hashedPassword",
                FullName = "Test Test",
                Is2FAEnabled = isTwoFactorEnabled,
                IsLocked = isLocked,
                FailedLoginAttempts = failedLoginAttempts,
                Preferred2FAMethod = "email"
            };
        }

        [Test]
        public void Login_InvalidEmailFormat_ReturnsFailure()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "invalidemail",
                Password = "imvalidpassword"
            };

            LoginResponse response = service.Login(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Invalid mail format."));
            authRepoMock.Verify(repo => repo.FindUserByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Login_UserDoesNotExist_ReturnsFailure()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password.3"
            };

            LoginResponse response = service.Login(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Invalid email or password."));
        }

        [Test]
        public void Login_IncorrectPassword_IncrementsFailedAttempts()
        {
            User user = CreateUser(failedLoginAttempts: 0);

            authRepoMock.Setup(repo => repo.FindUserByEmail(user.Email)).Returns(user);
            hashMock.Setup(hash => hash.Verify("wrongPassword", user.PasswordHash)).Returns(false);

            LoginRequest request = new LoginRequest
            {
                Email = user.Email,
                Password = "wrongPassword"
            };

            LoginResponse response = service.Login(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Invalid email or password."));
            authRepoMock.Verify(repo => repo.IncrementFailedAttempts(user.Id), Times.Once);
        }

        [Test]
        public void Login_MaxFailedAttemptsReached_LocksAccountAndSendsEmail()
        {
            User user = CreateUser(failedLoginAttempts: 4);

            authRepoMock.Setup(repo => repo.FindUserByEmail(user.Email)).Returns(user);
            hashMock.Setup(hash => hash.Verify("wrongPassword", user.PasswordHash)).Returns(false);

            LoginRequest request = new LoginRequest
            {
                Email = user.Email,
                Password = "wrongPassword"
            };

            LoginResponse response = service.Login(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Account locked due to too many failed attempts."));
            authRepoMock.Verify(repo => repo.LockAccount(user.Id, It.IsAny<DateTime>()), Times.Once);
            emailMock.Verify(email => email.SendLockNotification(user.Email), Times.Once);
        }

        [Test]
        public void Login_ValidCredentialsWithout2FA_ReturnsTokenAndSuccess()
        {
            User user = CreateUser(isTwoFactorEnabled: false);
            string expectedToken = "fake-jwt-token";

            authRepoMock.Setup(repo => repo.FindUserByEmail(user.Email)).Returns(user);
            hashMock.Setup(hash => hash.Verify("correctPassword", user.PasswordHash)).Returns(true);
            jwtMock.Setup(jwt => jwt.GenerateToken(user.Id)).Returns(expectedToken);

            LoginRequest request = new LoginRequest
            {
                Email = user.Email,
                Password = "correctPassword"
            };
            LoginResponse response = service.Login(request);

            Assert.That(response.Success, Is.True);
            Assert.That(response.Token, Is.EqualTo(expectedToken));
            Assert.That(response.Requires2FA, Is.False);
            Assert.That(response.UserId, Is.EqualTo(user.Id));
            authRepoMock.Verify(repo => repo.ResetFailedAttempts(user.Id), Times.Once);
            authRepoMock.Verify(repo => repo.CreateSession(user.Id, expectedToken, null, null, null), Times.Once);
            emailMock.Verify(email => email.SendLoginAlert(user.Email), Times.Once);
        }

        [Test]
        public void Login_ValidCredentialsWith2FAEnabled_ReturnsRequires2FA()
        {
            User user = CreateUser(isTwoFactorEnabled: true);
            string expectedOtp = "123456";

            authRepoMock.Setup(repo => repo.FindUserByEmail(user.Email)).Returns(user);
            hashMock.Setup(hash => hash.Verify("correctPassword", user.PasswordHash)).Returns(true);
            otpMock.Setup(otp => otp.GenerateTOTP(user.Id)).Returns(expectedOtp);

            LoginRequest request = new LoginRequest
            {
                Email = user.Email,
                Password = "correctPassword"
            };
            LoginResponse response = service.Login(request);

            Assert.That(response.Success, Is.True);
            Assert.That(response.Requires2FA, Is.True);
            Assert.That(response.Token, Is.Null);
            Assert.That(response.UserId, Is.EqualTo(user.Id));
            emailMock.Verify(email => email.SendOTPCode(user.Email, expectedOtp), Times.Once);
        }

        [Test]
        public void VerifyOTP_ValidOTP_ReturnsSuccessAndToken()
        {
            User user = CreateUser();
            string expectedToken = "fake-jwt-token";

            authRepoMock.Setup(repo => repo.FindUserById(user.Id)).Returns(user);
            otpMock.Setup(otp => otp.VerifyTOTP(user.Id, "123456")).Returns(true);
            jwtMock.Setup(jwt => jwt.GenerateToken(user.Id)).Returns(expectedToken);

            VerifyOTPRequest request = new VerifyOTPRequest
            {
                UserId = user.Id,
                OTPCode = "123456"
            };

            LoginResponse response = service.VerifyOTP(request);

            Assert.That(response.Success, Is.True);
            Assert.That(response.Token, Is.EqualTo(expectedToken));
            otpMock.Verify(otp => otp.InvalidateOTP(user.Id), Times.Once);
            authRepoMock.Verify(repo => repo.CreateSession(user.Id, expectedToken, null, null, null), Times.Once);
        }

        [Test]
        public void Register_ValidData_ReturnsSuccess()
        {
            authRepoMock.Setup(repo => repo.CreateUser(It.IsAny<User>())).Returns(true);
            hashMock.Setup(hash => hash.GetHash(It.IsAny<string>())).Returns("hashedPassword");

            RegisterRequest request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "ValidPassword123!",
                FullName = "New User"
            };

            RegisterResponse response = service.Register(request);

            Assert.That(response.Success, Is.True);
            Assert.That(response.Error, Is.Null);
            authRepoMock.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void Logout_ValidSessionToken_ReturnsTrueAndInvalidatesSession()
        {
            Session session = new Session { Id = 1, UserId = 1, Token = "valid-token" };
            authRepoMock.Setup(repo => repo.FindSessionByToken("valid-token")).Returns(session);

            bool result = service.Logout("valid-token");

            Assert.That(result, Is.True);
            authRepoMock.Verify(repo => repo.UpdateSessionToken(session.Id), Times.Once);
        }

        [Test]
        public void Register_InvalidEmailFormat_ReturnsFailure()
        {
            RegisterRequest request = new RegisterRequest
            {
                Email = "invalid-email",
                Password = "ValidPassword123!",
                FullName = "New User"
            };

            RegisterResponse response = service.Register(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Invalid email format."));
            authRepoMock.Verify(repo => repo.FindUserByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Register_WeakPassword_ReturnsFailure()
        {
            RegisterRequest request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "weak",
                FullName = "New User"
            };

            RegisterResponse response = service.Register(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Password must be at least 8 characters with uppercase, lowercase, and a digit."));
            authRepoMock.Verify(repo => repo.FindUserByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Register_MissingFullName_ReturnsFailure()
        {
            RegisterRequest request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "ValidPassword123!",
                FullName = " "
            };

            RegisterResponse response = service.Register(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Full name is required."));
            authRepoMock.Verify(repo => repo.FindUserByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Register_EmailAlreadyExists_ReturnsFailure()
        {
            User existingUser = CreateUser();

            authRepoMock.Setup(repo => repo.FindUserByEmail(existingUser.Email)).Returns(existingUser);

            RegisterRequest request = new RegisterRequest
            {
                Email = existingUser.Email,
                Password = "ValidPassword123!",
                FullName = "New User"
            };

            RegisterResponse response = service.Register(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Email is already registered."));
            authRepoMock.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void Register_DatabaseCreationFails_ReturnsFailure()
        {
            authRepoMock.Setup(repo => repo.CreateUser(It.IsAny<User>())).Returns(false);

            RegisterRequest request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "ValidPassword123!",
                FullName = "New User"
            };

            RegisterResponse response = service.Register(request);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Error, Is.EqualTo("Failed to create account."));
        }
    }
}
