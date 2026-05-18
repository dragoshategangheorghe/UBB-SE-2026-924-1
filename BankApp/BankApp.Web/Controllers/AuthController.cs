using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Auth;
using BankApp.Web.Infrastructure;
using BankApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    [AllowAnonymousSession]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IWebSessionContext _webSessionContext;

        public AuthController(IAuthService authService, IWebSessionContext webSessionContext)
        {
            _authService = authService;
            _webSessionContext = webSessionContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (_webSessionContext.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new LoginRequest { Email = model.Email, Password = model.Password };
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                model.ErrorMessage = "An error occurred connecting to the server.";
                return View(model);
            }

            if (!response.Success)
            {
                model.ErrorMessage = response.Error != null && response.Error.Contains("locked")
                    ? "Account is locked."
                    : "Invalid credentials.";
                return View(model);
            }

            if (response.Requires2FA)
            {
                return RedirectToAction("TwoFactor");
            }

            _webSessionContext.Authenticate(response.Token, response.UserId ?? 0);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult TwoFactor()
        {
            if (_authService.GetCurrentUserId() == null)
            {
                return RedirectToAction("Index");
            }
            return View(new TwoFactorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _authService.GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var request = new VerifyOTPRequest
            {
                UserId = userId.Value,
                OTPCode = model.Code
            };

            var response = await _authService.VerifyOtpAsync(request);

            if (response != null && response.Success)
            {
                _webSessionContext.Authenticate(response.Token, response.UserId ?? userId.Value);
                return RedirectToAction("Index", "Dashboard");
            }

            model.ErrorMessage = "Invalid or expired code.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new RegisterRequest
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password
            };
            var response = await _authService.RegisterAsync(request);

            if (response != null && response.Success)
            {
                model.SuccessMessage = "Account created! You can now sign in.";
                return View(model);
            }

            model.ErrorMessage = response?.Error ?? "Registration failed.";
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel { Step = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (model.Step == 1)
            {
                var req = new ForgotPasswordRequest { Email = model.Email };
                await _authService.ForgotPasswordAsync(req);
                model.Step = 2;
                return View(model);
            }

            if (model.Step == 2)
            {
                var isValid = await _authService.VerifyResetTokenAsync(model.Token);
                if (isValid)
                {
                    model.Step = 3;
                }
                else
                {
                    model.ErrorMessage = "Invalid recovery code.";
                }
                return View(model);
            }

            if (model.Step == 3)
            {
                var req = new ResetPasswordRequest
                {
                    Token = model.Token,
                    NewPassword = model.NewPassword
                };
                var response = await _authService.ResetPasswordAsync(req);
                if (response)
                {
                    model.SuccessMessage = "Password reset successfully. You can now login.";
                    model.Step = 4;
                }
                else
                {
                    model.ErrorMessage = "Failed to reset password.";
                }
                return View(model);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await _authService.LogoutAsync();
            _webSessionContext.Clear();
            return RedirectToAction("Index");
        }
    }
}