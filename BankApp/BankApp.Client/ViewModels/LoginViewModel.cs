using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Enums;
using Duende.IdentityModel.OidcClient;
using System;
using System.Threading.Tasks;

namespace BankApp.Client.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Observable<LoginState> State { get; private set; }
        private readonly ApiService _apiService;

        public LoginViewModel(ApiService apiService)
        {
            State = new Observable<LoginState>(LoginState.Idle);
            _apiService = apiService;
        }

        public async void Login(string email, string password)
        {
            SetState(State, LoginState.Loading);

            try
            {
                BankApp.Models.DTOs.Auth.LoginRequest request = new BankApp.Models.DTOs.Auth.LoginRequest
                {
                    Email = email,
                    Password = password
                };

                LoginResponse? response = await _apiService.PostAsync<BankApp.Models.DTOs.Auth.LoginRequest, LoginResponse>(
                    "/api/auth/login", request);

                if (response == null)
                {
                    SetState(State, LoginState.Error);
                    return;
                }

                if (!response.Success)
                {
                    HandleLoginError(response);
                    return;
                }

                if (response.Requires2FA)
                {
                    _apiService.SetCurrentUserId(response.UserId!.Value);

                    SetState(State, LoginState.Require2FA);
                    return;
                }

                // Login successful
                // Store the token and userId for future requests
                _apiService.SetToken(response.Token!);
                _apiService.SetCurrentUserId(response.UserId!.Value);
                SetState(State, LoginState.Success);
            }
            catch (Exception)
            {
                SetState(State, LoginState.Error);
            }
        }

        public async void OAuthLogin(string email, string provider)
        {
            SetState(State, LoginState.Loading);

            try
            {
                if (provider.ToLower() == "google")
                {
                    var options = new OidcClientOptions
                    {
                        Authority = "https://accounts.google.com",
                        ClientId = OAuthSecrets.ClientId,
                        ClientSecret = OAuthSecrets.ClientSecret,
                        Scope = "openid email profile",
                        RedirectUri = "http://127.0.0.1:7890/",
                        Browser = new BankApp.Client.Utilities.SystemBrowser(7890)
                    };

                    options.Policy.Discovery.ValidateEndpoints = false;

                    var oidcClient = new OidcClient(options);

                    var loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());

                    if (loginResult.IsError)
                    {
                        SetState(State, LoginState.Error);
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
                        SetState(State, LoginState.Error);
                        return;
                    }

                    if (response.Requires2FA)
                    {
                        _apiService.SetCurrentUserId(response.UserId!.Value);
                        SetState(State, LoginState.Require2FA);
                        return;
                    }

                    _apiService.SetToken(response.Token!);
                    _apiService.SetCurrentUserId(response.UserId!.Value);
                    SetState(State, LoginState.Success);
                }
            }
            catch (Exception ex)
            {
                SetState(State, LoginState.Error);
            }
        }

        private void HandleLoginError(LoginResponse response)
        {
            if (response.Error != null && response.Error.Contains("locked"))
            {
                SetState(State, LoginState.AccountLocked);
            }
            else
            {
                SetState(State, LoginState.InvalidCredentials);
            }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}