namespace BankApp.Models.Enums
{
    public enum TwoFactorState
    {
        Idle,
        Verifying,
        Success,
        InvalidOTP,
        Expired,
        MaxAttemptsReached
    }
}