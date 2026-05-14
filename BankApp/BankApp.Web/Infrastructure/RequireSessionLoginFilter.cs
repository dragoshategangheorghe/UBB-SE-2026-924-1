using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BankApp.Web.Infrastructure;

public sealed class RequireSessionLoginFilter : IAsyncActionFilter
{
    private readonly IWebSessionContext _sessionContext;

    public RequireSessionLoginFilter(IWebSessionContext sessionContext)
    {
        _sessionContext = sessionContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousSessionAttribute>().Any())
        {
            await next();
            return;
        }

        if (_sessionContext.IsAuthenticated)
        {
            await next();
            return;
        }

        string returnUrl = $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
        context.Result = new RedirectToActionResult("Index", "Auth", new { returnUrl });
    }
}
