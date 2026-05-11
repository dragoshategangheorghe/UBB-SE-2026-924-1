using System.Threading.Tasks;
using Moq;
using Xunit;
using BankApp.Client.Services.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Auth;

namespace BankApp.Client.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthRepoProxy> mockAuthRepoProxy;
        private readonly AuthService authService;

        public AuthServiceTests()
        {
            mockAuthRepoProxy = new Mock<IAuthRepoProxy>();
            authService = new AuthService(mockAuthRepoProxy.Object);
        }

        [Fact]
        public async Task LoginAsync_UnsuccessfulLogin_ReturnsOriginalFailedResponse()
        {
            LoginRequest loginRequestPayload = new LoginRequest { Email = "user@test.com", Password = "password" };
            LoginResponse failedLoginResponse = new LoginResponse { Success = false };

            mockAuthRepoProxy.Setup(proxy => proxy.LoginAsync(loginRequestPayload))
                .ReturnsAsync(failedLoginResponse);

            LoginResponse? returnedResponse = await authService.LoginAsync(loginRequestPayload);

            Assert.False(returnedResponse?.Success);
        }

        [Fact]
        public async Task LoginAsync_RequiresTwoFactorAuthentication_SetsCurrentUserIdButDoesNotSetToken()
        {
            LoginRequest loginRequestPayload = new LoginRequest { Email = "user@test.com", Password = "password" };
            int authenticatedUserId = 123;
            LoginResponse twoFactorRequiredResponse = new LoginResponse
            {
                Success = true,
                Requires2FA = true,
                UserId = authenticatedUserId
            };

            mockAuthRepoProxy.Setup(proxy => proxy.LoginAsync(loginRequestPayload))
                .ReturnsAsync(twoFactorRequiredResponse);

            await authService.LoginAsync(loginRequestPayload);

            mockAuthRepoProxy.Verify(proxy => proxy.SetCurrentUserId(authenticatedUserId), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_SuccessfulLogin_SetsBearerTokenAndUserId()
        {
            LoginRequest loginRequestPayload = new LoginRequest { Email = "user@test.com", Password = "password" };
            int authenticatedUserId = 123;
            string validAuthenticationToken = "valid_jwt_token";
            LoginResponse successfulLoginResponse = new LoginResponse
            {
                Success = true,
                Requires2FA = false,
                UserId = authenticatedUserId,
                Token = validAuthenticationToken
            };

            mockAuthRepoProxy.Setup(proxy => proxy.LoginAsync(loginRequestPayload))
                .ReturnsAsync(successfulLoginResponse);

            await authService.LoginAsync(loginRequestPayload);

            mockAuthRepoProxy.Verify(proxy => proxy.SetBearerToken(validAuthenticationToken), Times.Once);
        }

        [Fact]
        public async Task VerifyOtpAsync_SuccessfulVerification_SetsBearerToken()
        {
            VerifyOTPRequest verificationRequestPayload = new VerifyOTPRequest { UserId = 1, OTPCode = "123456" };
            string validAuthenticationToken = "valid_jwt_token";
            LoginResponse successfulVerificationResponse = new LoginResponse
            {
                Success = true,
                Token = validAuthenticationToken
            };

            mockAuthRepoProxy.Setup(proxy => proxy.VerifyOtpAsync(verificationRequestPayload))
                .ReturnsAsync(successfulVerificationResponse);

            await authService.VerifyOtpAsync(verificationRequestPayload);

            mockAuthRepoProxy.Verify(proxy => proxy.SetBearerToken(validAuthenticationToken), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_NoTokenPresent_ClearsLocalSessionDirectlyAndReturnsTrue()
        {
            mockAuthRepoProxy.Setup(proxy => proxy.GetBearerToken()).Returns(string.Empty);

            bool isLogoutSuccessful = await authService.LogoutAsync();

            Assert.True(isLogoutSuccessful);
        }

        [Fact]
        public async Task LogoutAsync_TokenIsPresent_InvokesPostLogoutAndClearsSession()
        {
            mockAuthRepoProxy.Setup(proxy => proxy.GetBearerToken()).Returns("existing_token");
            mockAuthRepoProxy.Setup(proxy => proxy.LogoutPostAsync()).ReturnsAsync(true);

            await authService.LogoutAsync();

            mockAuthRepoProxy.Verify(proxy => proxy.ClearLocalSession(), Times.Once);
        }
    }
}