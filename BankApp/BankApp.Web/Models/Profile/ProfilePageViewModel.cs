using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;

namespace BankApp.Web.Models.Profile;

public class ProfilePageViewModel
{
    // ── Data ────────────────────────────────────────────────
    public ProfileInfo? ProfileInfo { get; set; }
    public List<OAuthLink> OAuthLinks { get; set; } = [];
    public List<NotificationPreference> NotificationPreferences { get; set; } = [];

    // ── Active tab (personal | security | notifications) ────
    public string ActiveTab { get; set; } = TabPersonal;

    // ── Status messages (TempData round-trip) ───────────────
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    // ── Form sub-models ─────────────────────────────────────
    public UpdatePersonalInfoFormModel UpdatePersonalInfo { get; set; } = new();
    public VerifyPasswordFormModel VerifyPassword { get; set; } = new();
    public ChangePasswordFormModel ChangePassword { get; set; } = new();
    public Toggle2FAFormModel Toggle2FA { get; set; } = new();
    public NotificationPreferencesFormModel NotificationPrefs { get; set; } = new();

    // ── Tab name constants ───────────────────────────────────
    public const string TabPersonal = "personal";
    public const string TabSecurity = "security";
    public const string TabNotifications = "notifications";
}

// ── Form models ─────────────────────────────────────────────

public class UpdatePersonalInfoFormModel
{
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    /// <summary>Current password supplied by the user to authorise the edit.</summary>
    public string? VerifiedPassword { get; set; }
}

public class VerifyPasswordFormModel
{
    public string? Password { get; set; }
    /// <summary>Which flow triggered the verification: "edit" | "password" | "2fa"</summary>
    public string? Intent { get; set; }
}

public class ChangePasswordFormModel
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class Toggle2FAFormModel
{
    /// <summary>Desired state: true = enable, false = disable.</summary>
    public bool Enable { get; set; }
    /// <summary>Method: "Email" | "Phone"</summary>
    public string? Method { get; set; }
}

public class NotificationPreferencesFormModel
{
    /// <summary>
    /// Flat list of toggle updates submitted from the notifications tab.
    /// Each entry carries the preference Id plus the three channel booleans.
    /// </summary>
    public List<NotificationPrefUpdateItem> Items { get; set; } = [];
}

public class NotificationPrefUpdateItem
{
    public int Id { get; set; }
    public bool PushEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}