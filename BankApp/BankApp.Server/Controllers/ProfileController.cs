using Microsoft.AspNetCore.Mvc;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Utilities;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IHashService hashService;

        public ProfileController(IUserRepository userRepository, IHashService hashService)
        {
            this.userRepository = userRepository;
            this.hashService = hashService;
        }
        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        // GET: api/profile
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

        // PUT: api/profile
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            int userId = GetAuthenticatedUserId();
            request.UserId = userId; // override whatever the client sent

            if (request.UserId == null)
            {
                return BadRequest(new UpdateProfileResponse(false, "Something went wrong. Please try again."));
            }

            User? user = userRepository.FindById(userId);
            if (user == null)
            {
                return BadRequest(new UpdateProfileResponse(false, "User not found."));
            }

            if (request.PhoneNumber != null)
            {
                if (!ValidationUtil.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return BadRequest(new UpdateProfileResponse(false, "Invalid phone number."));
                }

                user.PhoneNumber = request.PhoneNumber;
            }

            if (request.Address != null)
            {
                user.Address = request.Address;
            }

            if (!userRepository.UpdateUser(user))
            {
                return BadRequest(new UpdateProfileResponse(false, "Could not update user."));
            }

            return Ok(new UpdateProfileResponse(true, "User profile updated successfully."));
        }

        // PUT: api/profile/password
        [HttpPut("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            int userId = GetAuthenticatedUserId();
            request.UserId = userId; // override whatever the client sent

            User? user = userRepository.FindById(request.UserId);
            if (user == null)
            {
                return BadRequest(new ChangePasswordResponse(false, "User not found."));
            }

            if (!hashService.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new ChangePasswordResponse(false, "Current password is incorrect. Please try again."));
            }

            user.PasswordHash = hashService.GetHash(request.NewPassword);
            userRepository.UpdatePassword(user.Id, user.PasswordHash);
            return Ok(new ChangePasswordResponse(true, "Password changed successfully."));
        }

        // GET: api/profile/oauthlinks
        [HttpGet("oauthlinks")]
        public IActionResult GetOAuthLinks()
        {
            int userId = GetAuthenticatedUserId();

            List<OAuthLink> links = userRepository.GetLinkedProviders(userId);

            // Empty list is normal — return 200 so clients do not treat "no links" as an error.
            return Ok(links);
        }

        // GET: api/profile/notifications/preferences
        [HttpGet("notifications/preferences")]
        public IActionResult GetNotificationPreferences()
        {
            int userId = GetAuthenticatedUserId();

            List<NotificationPreference> prefs = userRepository.GetNotificationPreferences(userId);

            // Empty list is normal — return 200 so profile load does not fail for new users.
            return Ok(prefs);
        }

        // PUT: api/profile/notifications/preferences
        [HttpPut("notifications/preferences")]
        public IActionResult UpdateNotificationPreferences([FromBody] List<NotificationPreference>? prefs)
        {
            int userId = GetAuthenticatedUserId();

            if (prefs == null)
            {
                return BadRequest(false);
            }

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

            User? user = userRepository.FindById(userId);
            if (user == null)
            {
                return BadRequest(false);
            }

            bool success = hashService.Verify(password, user.PasswordHash);

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

            User? user = userRepository.FindById(userId);
            if (user == null)
            {
                return BadRequest(new Toggle2FAResponse { Success = false });
            }

            user.Is2FAEnabled = true;
            user.Preferred2FAMethod = request.Method.ToString();
            bool success = userRepository.UpdateUser(user);

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

            User? user = userRepository.FindById(userId);
            if (user == null)
            {
                return BadRequest(new Toggle2FAResponse { Success = false });
            }

            user.Is2FAEnabled = false;
            user.Preferred2FAMethod = null;
            bool success = userRepository.UpdateUser(user);

            if (!success)
            {
                return BadRequest(new Toggle2FAResponse { Success = false });
            }

            return Ok(new Toggle2FAResponse { Success = true });
        }
    }
}