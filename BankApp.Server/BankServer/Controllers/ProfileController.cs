using Microsoft.AspNetCore.Mvc;
using BankApp.Server.Services.Interfaces;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService profileService;
        public ProfileController(IProfileService profileService)
        {
            this.profileService = profileService;
        }
        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        // GET: api/profile
        [HttpGet]
        public IActionResult GetProfile()
        {
            int userId = GetAuthenticatedUserId();

            User? user = profileService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new GetProfileResponse(false, "User not found."));
            }

            return Ok(new GetProfileResponse(true, "Successfully retrieved profile information.", user));
        }

        // PUT: api/profile
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            int userId = GetAuthenticatedUserId();
            request.UserId = userId; // override whatever the client sent

            UpdateProfileResponse response = profileService.UpdatePersonalInfo(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // PUT: api/profile/password
        [HttpPut("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            int userId = GetAuthenticatedUserId();
            request.UserId = userId; // override whatever the client sent

            ChangePasswordResponse response = profileService.ChangePassword(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // GET: api/profile/oauthlinks
        [HttpGet("oauthlinks")]
        public IActionResult GetOAuthLinks()
        {
            int userId = GetAuthenticatedUserId();

            List<OAuthLink> links = profileService.GetOAuthLinks(userId);

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

            List<NotificationPreference> prefs = profileService.GetNotificationPreferences(userId);

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

            bool success = profileService.UpdateNotificationPreferences(userId, prefs);

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

            bool success = profileService.VerifyPassword(userId, password);

            if (!success)
            {
                return BadRequest(false);
            }

            return Ok(true);
        }

        // PUT: api/profile/2fa/enable
        [HttpPut("2fa/enable")]
        public IActionResult Enable2FA([FromBody] Enable2FARequest request)
        {
            int userId = GetAuthenticatedUserId();

            bool success = profileService.Enable2FA(userId, request.Method);

            if (!success)
            {
                return BadRequest(new Toggle2FAResponse { Success = false });
            }

            return Ok(new Toggle2FAResponse { Success = true });
        }

        // PUT: api/profile/2fa/disable
        [HttpPut("2fa/disable")]
        public IActionResult Disable2FA()
        {
            int userId = GetAuthenticatedUserId();

            bool success = profileService.Disable2FA(userId);

            if (!success)
            {
                return BadRequest(new Toggle2FAResponse { Success = false });
            }

            return Ok(new Toggle2FAResponse { Success = true });
        }
    }
}