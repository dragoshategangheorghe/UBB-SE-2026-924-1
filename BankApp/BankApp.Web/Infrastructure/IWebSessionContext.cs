namespace BankApp.Web.Infrastructure;

public interface IWebSessionContext
{
    string? AccessToken { get; }

    int? CurrentUserId { get; }

    bool IsAuthenticated { get; }

    void Clear();
}
