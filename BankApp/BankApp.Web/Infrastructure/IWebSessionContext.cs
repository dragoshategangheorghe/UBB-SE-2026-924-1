namespace BankApp.Web.Infrastructure;

public interface IWebSessionContext
{
    string? AccessToken { get; }

    int? CurrentUserId { get; }

    bool IsAuthenticated { get; }

    void Authenticate(string accessToken, int currentUserId);

    void Clear();
}
