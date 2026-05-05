using System.Security.Cryptography;
using System.Text;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BankApp.Server.Services.Infrastructure.Implementations
{
    public class OTPService : IOTPService
    {
        private static readonly Dictionary<int, (string Code, DateTime ExpiryTime)> TemporarySmsStorage = new ();
        private const int SmsOtpExpiryMinutes = 5;
        private const int TotpWindowSeconds = 300;

        public OTPService()
        {
        }

        public string GenerateSMSOTP(int userId)
        {
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(SmsOtpExpiryMinutes);
            TemporarySmsStorage[userId] = (code, expiryTime);
            return code;
        }

        public string GenerateTOTP(int userId)
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            return GenerateHmacCode(userId, currentWindow);
        }

        public void InvalidateOTP(int userId)
        {
            TemporarySmsStorage.Remove(userId);
        }

        public bool IsExpired(DateTime expiredAt)
        {
            return DateTime.UtcNow > expiredAt;
        }

        public bool VerifySMSOTP(int userId, string code)
        {
            if (TemporarySmsStorage.TryGetValue(userId, out var storedData))
            {
                if (DateTime.UtcNow > storedData.ExpiryTime)
                {
                    InvalidateOTP(userId);
                    return false;
                }
                if (storedData.Code == code)
                {
                    InvalidateOTP(userId);
                    return true;
                }
            }
            return false;
        }

        public bool VerifyTOTP(int userId, string code)
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            if (code == GenerateHmacCode(userId, currentWindow))
            {
                return true;
            }

            if (code == GenerateHmacCode(userId, currentWindow - 1))
            {
                return true;
            }

            return false;
        }

        private string GenerateHmacCode(int userId, long timeWindow)
        {
            string secret = $"User_Secret_Key_{userId}_BankApp";
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(BitConverter.GetBytes(timeWindow));
            int offset = hash[hash.Length - 1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24) |
                         ((hash[offset + 1] & 0xFF) << 16) |
                         ((hash[offset + 2] & 0xFF) << 8) |
                         (hash[offset + 3] & 0xFF);
            return (binary % 1000000).ToString("D6");
        }
    }
}
