using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;

namespace BankApp.Web.Models.Profile;

public class ProfilePageViewModel
{
    public ProfileInfo? ProfileInfo { get; set; }
    public List<OAuthLink> OAuthLinks { get; set; } = [];
    public List<NotificationPreference> NotificationPreferences { get; set; } = [];

    
    public string ActiveTab { get; set; } = TabPersonal;

    
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    
    public UpdatePersonalInfoFormModel UpdatePersonalInfo { get; set; } = new();
    public VerifyPasswordFormModel VerifyPassword { get; set; } = new();
    public ChangePasswordFormModel ChangePassword { get; set; } = new();
    public Toggle2FAFormModel Toggle2FA { get; set; } = new();
    public NotificationPreferencesFormModel NotificationPrefs { get; set; } = new();

    
    public const string TabPersonal = "personal";
    public const string TabSecurity = "security";
    public const string TabNotifications = "notifications";
}



public class UpdatePersonalInfoFormModel
{
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? VerifiedPassword { get; set; }
}

public class VerifyPasswordFormModel
{
    public string? Password { get; set; }
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
    public bool Enable { get; set; }
    public string? Method { get; set; }
}

public class NotificationPreferencesFormModel
{
    public List<NotificationPrefUpdateItem> Items { get; set; } = [];
}

public class NotificationPrefUpdateItem
{
    public int Id { get; set; }
    public bool PushEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}