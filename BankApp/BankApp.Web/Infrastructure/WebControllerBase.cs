using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Infrastructure;

public abstract class WebControllerBase : Controller
{
    protected WebControllerBase(IWebSessionContext sessionContext)
    {
        SessionContext = sessionContext;
    }

    protected IWebSessionContext SessionContext { get; }

    protected int CurrentUserId =>
        SessionContext.CurrentUserId
        ?? throw new InvalidOperationException("No authenticated user is available.");

    protected bool TryHandleUnauthorized(HttpRequestException exception, out IActionResult result)
    {
        if (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            SessionContext.Clear();
            string returnUrl = $"{Request.Path}{Request.QueryString}";
            result = RedirectToAction("Index", "Auth", new { returnUrl });
            return true;
        }

        result = null!;
        return false;
    }
}
