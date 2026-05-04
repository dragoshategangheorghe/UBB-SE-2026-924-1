namespace BankApp.Models.Enums
{
    public enum LoginState
    {
        Idle,
        Loading,
        Success,
        Require2FA,
        InvalidCredentials,
        AccountLocked,
        Error
    }
}