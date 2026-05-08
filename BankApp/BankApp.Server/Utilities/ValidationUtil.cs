using System.Text.RegularExpressions;
using System.Net.Mail;

namespace BankApp.Server.Utilities
{
    public static class ValidationUtil
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            email = email.Trim().ToLower();

            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        public static bool IsValidOTP(string otp)
        {
            return !string.IsNullOrWhiteSpace(otp) && otp.Length == 6 && otp.All(char.IsDigit);
        }

        public static bool PasswordsMatch(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a == b;
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return Regex.IsMatch(phone, @"^\+?[\d\s\-().]{7,15}$");
        }
    }
}