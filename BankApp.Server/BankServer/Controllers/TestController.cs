using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    // This controller is just used for testing endpoints, have fun :) <3 B
    // THIS SHOULD NOT GO INTO PRODUCTION FOR ANY REASON WHATSOEVER, AFTER WE ARE DONE
    // WITH TESTING WE CAN DELETE THIS FILE AND NOT CARE ABOUT MERGE CONFLICTS HERE
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("db")]
        public IActionResult TestDb([FromServices] AppDbContext db)
        {
            try
            {
                var reader = db.ExecuteQuery("SELECT COUNT(*) FROM [User]", Array.Empty<object>());
                reader.Read();
                var count = reader.GetInt32(0);
                reader.Close();
                return Ok(new { message = "Connection works!", userCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet("user/find/{email}")]
        public IActionResult FindUser([FromServices] IUserDAO userDao, string email)
        {
            try
            {
                var user = userDao.FindByEmail(email);
                if (user == null)
                {
                    return NotFound(new { error = $"User with email '{email}' not found" });
                }

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.PhoneNumber,
                    user.Is2FAEnabled,
                    user.IsLocked,
                    user.FailedLoginAttempts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("hash/{text}")]
        public IActionResult TestHash([FromServices] IHashService hash, string text)
        {
            try
            {
                var hashed = hash.GetHash(text);
                var verified = hash.Verify(text, hashed);
                return Ok(new { original = text, hash = hashed, verified });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("jwt/generate/{userId}")]
        public IActionResult TestJwtGenerate([FromServices] IJWTService jwt, int userId)
        {
            try
            {
                var token = jwt.GenerateToken(userId);
                return Ok(new { userId, token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("jwt/validate/{token}")]
        public IActionResult TestJwtValidate([FromServices] IJWTService jwt, string token)
        {
            try
            {
                var userId = jwt.ExtractUserId(token);
                return Ok(new { valid = userId != null, userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
