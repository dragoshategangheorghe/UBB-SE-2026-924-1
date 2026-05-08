using System.Net;
using System.Net.Mail;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BankApp.Server.Services.Infrastructure.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration config;
        public EmailService(IConfiguration config)
        {
            this.config = config;
        }

        public void SendLockNotification(string email)
        {
            string subject = "BankApp - Account Locked";
            string body = "Hello,\n\nYour account has been temporarily locked due to multiple failed login attempts. Please try again later or reset your password.";
            SendEmail(email, subject, body);
        }

        public void SendLoginAlert(string email)
        {
            string subject = "BankApp - New Login Detected";
            string body = "Hello,\n\nWe detected a new login to your BankApp account. If this was you, no action is needed. If this wasn't you, please change your password immediately.";
            SendEmail(email, subject, body);
        }

        public void SendOTPCode(string email, string code)
        {
            string subject = "Your BankApp Login Code";
            string body = $"Hello,\n\nYour One-Time Password (OTP) is: {code}\n\nThis code is valid for 5 minutes. Do not share it with anyone.";
            SendEmail(email, subject, body);
        }

        public void SendPasswordResetLink(string email, string token)
        {
            string subject = "BankApp - Password Reset Code";
            string body = $"Hello,\n\nYou requested a password reset. Please copy and paste the recovery code below into the app:\n\n{token}\n\nIf you did not request this, please ignore this email.";
            SendEmail(email, subject, body);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                string host = config["Email:SmtpHost"] ?? throw new Exception("SMTP Host is missing.");
                int port = int.Parse(config["Email:SmtpPort"] ?? "587");
                string user = config["Email:SmtpUser"] ?? throw new Exception("SMTP User is missing.");
                string pass = config["Email:SmtpPass"] ?? throw new Exception("SMTP Password is missing.");
                string from = config["Email:FromAddress"] ?? user;
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = true,
                    UseDefaultCredentials = false
                };
                using var mailMessage = new MailMessage(from, toEmail, subject, body);
                client.Send(mailMessage);
            }
            catch (Exception)
            {
            }
        }
    }
}
