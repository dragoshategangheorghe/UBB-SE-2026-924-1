using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Web.Infrastructure;
using BankApp.Web.Models.Profile;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers;

//[Authorize]
public class ProfileController : WebControllerBase
{
    private const int MinimumPasswordLength = 8;

    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService, IWebSessionContext sessionContext)
        : base(sessionContext)
    {
        _profileService = profileService;
    }

    // ══════════════════════════════════════════════════════════
    //  GET  /Profile
    // ══════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> Index(string tab = ProfilePageViewModel.TabPersonal)
    {
        try
        {
            var model = await BuildPageModelAsync(tab);
            return View(model);
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            return View(new ProfilePageViewModel
            {
                ActiveTab = tab,
                ErrorMessage = exception.Message,
            });
        }
    }

    // ══════════════════════════════════════════════════════════
    //  POST  /Profile/UpdatePersonalInfo
    // ══════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePersonalInfo(
        [Bind(Prefix = "UpdatePersonalInfo")] UpdatePersonalInfoFormModel form)
    {
        try
        {
            // Verify the password first
            if (string.IsNullOrWhiteSpace(form.VerifiedPassword))
            {
                TempData["ErrorMessage"] = "Password verification is required before saving changes.";
                return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabPersonal });
            }

            bool verified = await _profileService.VerifyPasswordAsync(form.VerifiedPassword);
            if (!verified)
            {
                TempData["ErrorMessage"] = "Incorrect password. Changes were not saved.";
                return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabPersonal });
            }

            var request = new UpdateProfileRequest(
                CurrentUserId,
                string.IsNullOrWhiteSpace(form.PhoneNumber) ? null : form.PhoneNumber.Trim(),
                string.IsNullOrWhiteSpace(form.Address) ? null : form.Address.Trim());

            var response = await _profileService.UpdateProfileAsync(request);

            if (response?.Success == true)
            {
                TempData["StatusMessage"] = "Profile updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Failed to update profile.";
            }
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabPersonal });
    }

    // ══════════════════════════════════════════════════════════
    //  POST  /Profile/ChangePassword
    // ══════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        [Bind(Prefix = "ChangePassword")] ChangePasswordFormModel form)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(form.CurrentPassword) ||
                string.IsNullOrWhiteSpace(form.NewPassword) ||
                string.IsNullOrWhiteSpace(form.ConfirmPassword))
            {
                TempData["ErrorMessage"] = "All password fields are required.";
                return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabSecurity });
            }

            if (form.NewPassword.Length < MinimumPasswordLength)
            {
                TempData["ErrorMessage"] = $"New password must be at least {MinimumPasswordLength} characters.";
                return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabSecurity });
            }

            if (form.NewPassword != form.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New passwords do not match.";
                return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabSecurity });
            }

            var request = new ChangePasswordRequest(CurrentUserId, form.CurrentPassword, form.NewPassword);
            var response = await _profileService.ChangePasswordAsync(request);

            if (response?.Success == true)
            {
                TempData["StatusMessage"] = "Password changed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Failed to change password.";
            }
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabSecurity });
    }

    // ══════════════════════════════════════════════════════════
    //  POST  /Profile/Toggle2FA
    // ══════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle2FA(
        [Bind(Prefix = "Toggle2FA")] Toggle2FAFormModel form)
    {
        try
        {
            Toggle2FAResponse? response;

            if (form.Enable)
            {
                var method = string.Equals(form.Method, "Phone", StringComparison.OrdinalIgnoreCase)
                    ? TwoFactorMethod.Phone
                    : TwoFactorMethod.Email;

                response = await _profileService.Enable2FAAsync(method);
            }
            else
            {
                response = await _profileService.Disable2FAAsync();
            }

            if (response?.Success == true)
            {
                TempData["StatusMessage"] = form.Enable ? "Two-factor authentication enabled." : "Two-factor authentication disabled.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update two-factor authentication settings.";
            }
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabSecurity });
    }

    // ══════════════════════════════════════════════════════════
    //  POST  /Profile/UpdateNotificationPreferences
    // ══════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotificationPreferences(
        [Bind(Prefix = "NotificationPrefs")] NotificationPreferencesFormModel form)
    {
        try
        {
            // Re-fetch the current preferences so we can patch only the submitted toggles
            var existing = await _profileService.GetNotificationPreferencesAsync() ?? [];

            foreach (var item in form.Items)
            {
                var pref = existing.FirstOrDefault(p => p.Id == item.Id);
                if (pref == null)
                {
                    continue;
                }

                pref.EmailEnabled = item.EmailEnabled;
                pref.SmsEnabled = item.SmsEnabled;
                pref.PushEnabled = item.PushEnabled;
            }

            bool success = await _profileService.UpdateNotificationPreferencesAsync(existing);

            TempData[success ? "StatusMessage" : "ErrorMessage"] =
                success ? "Notification preferences saved." : "Failed to save notification preferences.";
        }
        catch (HttpRequestException exception) when (TryHandleUnauthorized(exception, out var result))
        {
            return result;
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = ProfilePageViewModel.TabNotifications });
    }

    // ══════════════════════════════════════════════════════════
    //  Private helpers
    // ══════════════════════════════════════════════════════════

    private async Task<ProfilePageViewModel> BuildPageModelAsync(string tab)
    {
        var profileResponse = await _profileService.GetProfileAsync();
        var oauthLinks = await _profileService.GetOAuthLinksAsync() ?? [];
        var notificationPrefs = await _profileService.GetNotificationPreferencesAsync() ?? [];

        var model = new ProfilePageViewModel
        {
            ProfileInfo = profileResponse?.ProfileInfo,
            OAuthLinks = oauthLinks,
            NotificationPreferences = notificationPrefs,
            ActiveTab = tab,
            StatusMessage = TempData["StatusMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string,
        };

        // Pre-populate the personal info form with current values
        if (model.ProfileInfo != null)
        {
            model.UpdatePersonalInfo = new UpdatePersonalInfoFormModel
            {
                PhoneNumber = model.ProfileInfo.PhoneNumber,
                Address = model.ProfileInfo.Address,
            };
        }

        return model;
    }
}