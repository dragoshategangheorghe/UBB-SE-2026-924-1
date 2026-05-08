namespace BankApp.Server.Services.Infrastructure.Interfaces;

public interface IHashService
{
    string GetHash(string input);
    bool Verify(string input, string hash);
}