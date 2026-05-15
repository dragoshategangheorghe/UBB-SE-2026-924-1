using System;
using Microsoft.AspNetCore.Http;

namespace BankApp.Web.Infrastructure;

public sealed class WebSessionContext : IWebSessionContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebSessionContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? AccessToken => Session.GetString(WebSessionKeys.AccessToken);

    public int? CurrentUserId => Session.GetInt32(WebSessionKeys.CurrentUserId);

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && CurrentUserId.HasValue;

    public void Clear()
    {
        Session.Remove(WebSessionKeys.AccessToken);
        Session.Remove(WebSessionKeys.CurrentUserId);
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("An active HTTP session is required.");
}
