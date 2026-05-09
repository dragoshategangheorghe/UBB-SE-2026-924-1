using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Implementations;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        // GET: api/user
        [HttpGet]
        public IActionResult GetProfile()
        {
            int userId = GetAuthenticatedUserId();

            User? user = userRepository.FindById(userId);
            if (user == null)
            {
                return NotFound(new GetProfileResponse(false, "User not found."));
            }

            return Ok(new GetProfileResponse(true, "Successfully retrieved profile information.", user));
        }

        // PUT: api/user
        [HttpPut]
        public IActionResult UpdateUser([FromBody] User user)
        {
            int userId = GetAuthenticatedUserId();
            user.Id = userId; // it will NOT be given by the client, input it manually

            bool response = userRepository.UpdateUser(user); // successfully updated or not

            return Ok(response);
        }

        // PUT: api/profile/password
        [HttpPut("password")]
        public IActionResult UpdatePassword([FromBody] ChangePasswordRequest request)
        {
            Models.Entities.User? user = userRepository.FindById(request.UserId);

            userRepository.FindById(request.UserId).PasswordHash = HashService.GetHash(request.NewPassword);
            bool response = userRepository.UpdatePassword(request.UserId, user.PasswordHash);

            return Ok(response);
        }

        // GET: api/profile/oauthlinks
        [HttpGet("oauthlinks")]
        public IActionResult GetOAuthLinks()
        {
            int userId = GetAuthenticatedUserId();

            List<OAuthLink> links = userRepository.GetLinkedProviders(userId);

            if (links.Count == 0)
            {
                return NotFound(links);
            }

            return Ok(links);
        }

        // GET: api/profile/notifications/preferences
        [HttpGet("notifications/preferences")]
        public IActionResult GetNotificationPreferences()
        {
            int userId = GetAuthenticatedUserId();

            List<NotificationPreference> prefs = userRepository.GetNotificationPreferences(userId);

            if (prefs.Count == 0)
            {
                return NotFound(prefs);
            }

            return Ok(prefs);
        }

        // PUT: api/profile/notifications/preferences
        [HttpPut("notifications/preferences")]
        public IActionResult UpdateNotificationPreferences([FromBody] List<NotificationPreference> prefs)
        {
            int userId = GetAuthenticatedUserId();

            bool success = userRepository.UpdateNotificationPreferences(userId, prefs);

            if (!success)
            {
                return BadRequest(false);
            }

            return Ok(true);
        }

        // POST: api/profile/verify-password
        [HttpPost("verify-password")]
        public IActionResult VerifyPassword([FromBody] string password)
        {
            int userId = GetAuthenticatedUserId();
            Models.Entities.User user = userRepository.FindById(userId);

            bool success = HashService.Verify(password, user.PasswordHash);

            if (!success)
            {
                return BadRequest(false);
            }

            return Ok(true);
        }
    }
}