using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;

namespace BankApp.Server.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IAuthRepository authRepository, IJWTService jwtService)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Public endpoints, no token needed
            if (IsPublicEndpoint(path))
            {
                await next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            // No token provided
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                await RejectRequest(context, "No token provided.");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length);

            // Check if JWT valid
            var userId = jwtService.ExtractUserId(token);
            if (userId == null)
            {
                await RejectRequest(context, "Invalid or expired token.");
                return;
            }

            // check if session still active in the DB
            var session = authRepository.FindSessionByToken(token);
            if (session == null)
            {
                await RejectRequest(context, "Session expired or revoked.");
                return;
            }

            // Store userId so controllers can use it
            context.Items["UserId"] = userId;

            await next(context);
        }

        private bool IsPublicEndpoint(string? path)
        {
            if (path == null)
            {
                return false;
            }

            return path.Contains("/auth/")
                || path.Contains("/swagger")
                || path.Contains("/test/");
        }

        private async Task RejectRequest(HttpContext context, string error)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error });
        }
    }
}