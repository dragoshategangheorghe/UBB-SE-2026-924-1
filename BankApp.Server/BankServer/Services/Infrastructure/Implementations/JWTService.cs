using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BankApp.Server.Services.Infrastructure.Implementations
{
    public class JWTService : IJWTService
    {
        private readonly string secret;
        public JWTService(string secret)
        {
            this.secret = secret;
        }

        public string GenerateToken(int userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userId.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = key
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public int? ExtractUserId(string token)
        {
            var principal = ValidateToken(token);
            var claim = principal?.FindFirst("userId");

            if (claim != null && int.TryParse(claim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
