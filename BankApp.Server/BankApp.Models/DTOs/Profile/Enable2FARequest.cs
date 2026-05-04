using BankApp.Models.Enums;

namespace BankApp.Models.DTOs.Profile;

public class Enable2FARequest
{
    public TwoFactorMethod Method { get; set; }
}