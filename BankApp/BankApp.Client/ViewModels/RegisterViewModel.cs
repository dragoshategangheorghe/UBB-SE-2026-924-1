using BankApp.Client.Utilities;

using BankApp.Models.Enums;
using BankApp.Models.DTOs.Auth;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BankApp.Client.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        public Observable<RegisterState> State { get; private set; }

        private readonly ApiService _apiService;

        public RegisterViewModel(ApiService apiService)
        {
            State = new Observable<RegisterState>(RegisterState.Idle);
            _apiService = apiService;
        }

        public async void Register(string email, string password, string confirmPassword, string fullName)
        {
            // Client-side validation
            RegisterState? validationError = ValidateLocally(email, password, confirmPassword, fullName);
            if (validationError != null)
            {
                SetState(State, validationError.Value);
                return;
            }

            SetState(State, RegisterState.Loading);

            try
            {
                RegisterRequest? request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    FullName = fullName
                };

                RegisterResponse? response = await _apiService.PostAsync<RegisterRequest, RegisterResponse>(
                    "/api/auth/register", request);

                if (response == null)
                {
                    SetState(State, RegisterState.Error);
                    return;
                }

                if (!response.Success)
                {
                    HandleRegisterError(response);
                    return;
                }

                SetState(State, RegisterState.Success);
            }
            catch (Exception)
            {
                SetState(State, RegisterState.Error);
            }
        }

        public async void OAuthRegister(string email, string provider)
        {
            SetState(State, RegisterState.Loading);

            try
            {
                if (provider.ToLower() == "google")
                {
                    var options = new Duende.IdentityModel.OidcClient.OidcClientOptions
                    {
                        Authority = "https://accounts.google.com",
                        ClientId = OAuthSecrets.ClientId,
                        ClientSecret = OAuthSecrets.ClientSecret,
                        Scope = "openid email profile",
                        RedirectUri = "http://127.0.0.1:7890/",
                        Browser = new BankApp.Client.Utilities.SystemBrowser(7890)
                    };
                    options.Policy.Discovery.ValidateEndpoints = false;

                    var oidcClient = new Duende.IdentityModel.OidcClient.OidcClient(options);
                    var loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());

                    if (loginResult.IsError)
                    {
                        SetState(State, RegisterState.Error);
                        return;
                    }

                    OAuthLoginRequest apiRequest = new OAuthLoginRequest
                    {
                        Provider = "Google",
                        ProviderToken = loginResult.IdentityToken
                    };

                    LoginResponse? response = await _apiService.PostAsync<OAuthLoginRequest, LoginResponse>(
                        "/api/auth/oauth-login", apiRequest);

                    if (response == null || !response.Success)
                    {
                        SetState(State, RegisterState.Error);
                        return;
                    }

                    _apiService.SetToken(response.Token!);
                    _apiService.SetCurrentUserId(response.UserId!.Value);

                    SetState(State, RegisterState.AutoLoggedIn);
                }
            }
            catch (Exception)
            {
                SetState(State, RegisterState.Error);
            }
        }

        private RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) { return RegisterState.Error; }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) { return RegisterState.InvalidEmail; }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8
                || !password.Any(char.IsUpper)
                || !password.Any(char.IsLower)
                || !password.Any(char.IsDigit))
                { return RegisterState.WeakPassword; }

            if (password != confirmPassword) { return RegisterState.PasswordMismatch; }
            return null;
        }

        private void HandleRegisterError(RegisterResponse response)
        {
            if (response.Error != null && response.Error.Contains("already registered"))
                SetState(State, RegisterState.EmailAlreadyExists);
            else if (response.Error != null && response.Error.Contains("email"))
                SetState(State, RegisterState.InvalidEmail);
            else if (response.Error != null && response.Error.Contains("Password"))
                SetState(State, RegisterState.WeakPassword);
            else
                SetState(State, RegisterState.Error);
        }

        public override void Dispose()
        {
            // Clean up observers if needed
        }
    }
}