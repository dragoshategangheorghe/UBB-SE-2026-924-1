namespace BankApp.Models.Enums
{
    public enum RegisterState
    {
        Idle,
        Loading,
        Success,
        EmailAlreadyExists,
        InvalidEmail,
        WeakPassword,
        PasswordMismatch,
        Error,
        AutoLoggedIn
    }
}