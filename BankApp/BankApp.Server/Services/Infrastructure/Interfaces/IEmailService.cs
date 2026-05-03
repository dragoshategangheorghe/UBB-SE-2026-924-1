namespace BankApp.Server.Services.Infrastructure.Interfaces;

public interface IEmailService
{
    void SendPasswordResetLink(string email, string token);
    void SendOTPCode(string email, string code);
    void SendLoginAlert(string email);
    void SendLockNotification(string email);
}