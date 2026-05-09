using BankApp.Server.Services.Infrastructure.Interfaces;

namespace BankApp.Server.Services.Infrastructure.Implementations
{
    public static class HashService
    {
        static public string GetHash(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }

        static public bool Verify(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }
    }
}
