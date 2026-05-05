namespace BankApp.Models.Enums
{
    public enum ForgotPasswordState
    {
        Idle,
        EmailSent,
        TokenValid,
        TokenExpired,
        TokenAlreadyUsed,
        PasswordResetSuccess,
        Error
    }
}