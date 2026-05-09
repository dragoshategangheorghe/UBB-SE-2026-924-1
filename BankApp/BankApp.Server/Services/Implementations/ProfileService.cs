using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Services.Interfaces;
using BankApp.Server.Utilities;

namespace BankApp.Server.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHashService _hashService;

        public ProfileService(IUserRepository userRepository, IHashService hashService)
        {
            this._userRepository = userRepository;
            this._hashService = hashService;
        }

        public User? GetUserById(int userId)
        {
            return _userRepository.FindById(userId);
        }

        public UpdateProfileResponse UpdatePersonalInfo(UpdateProfileRequest request)
        {
            if (request.UserId == null)
            {
                return new UpdateProfileResponse(false, "Something went wrong. Please try again.");
            }

            int userId = request.UserId.Value;

            User? user = _userRepository.FindById(userId);
            if (user == null)
            {
                return new UpdateProfileResponse(false, "User not found.");
            }

            // Check and update phone number
            if (request.PhoneNumber != null)
            {
                if (!ValidationUtil.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return new UpdateProfileResponse(false, "Invalid phone number.");
                }

                user.PhoneNumber = request.PhoneNumber;
            }

            // Check and update address
            if (request.Address != null)
            {
                user.Address = request.Address;
            }

            // Update the user in the repo
            if (_userRepository.UpdateUser(user) == false)
            {
                return new UpdateProfileResponse(false, "Could not update user.");
            }

            return new UpdateProfileResponse(true, "User profile updated successfully.");
        }

        public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
        {
            User? user = _userRepository.FindById(request.UserId);
            if (user == null)
            {
                // Just making sure, should never happen though
                return new ChangePasswordResponse(false, "User not found.");
            }

            if (_hashService.Verify(request.CurrentPassword, user.PasswordHash))
            {
                user.PasswordHash = _hashService.GetHash(request.NewPassword);
                _userRepository.UpdatePassword(user.Id, user.PasswordHash);
                return new ChangePasswordResponse(true, "Password changed successfully.");
            }
            else
            {
                return new ChangePasswordResponse(false, "Current password is incorrect. Please try again.");
            }
        }

        public bool Enable2FA(int userId, TwoFactorMethod method)
        {
            User? user = _userRepository.FindById(userId);
            if (user == null)
            {
                return false;
            }

            user.Is2FAEnabled = true;
            user.Preferred2FAMethod = method.ToString();
            return _userRepository.UpdateUser(user);
        }

        public bool Disable2FA(int userId)
        {
            User? user = _userRepository.FindById(userId);
            if (user == null)
            {
                return false;
            }

            user.Is2FAEnabled = false;
            user.Preferred2FAMethod = null;
            return _userRepository.UpdateUser(user);
        }

        public List<OAuthLink> GetOAuthLinks(int userId)
        {
            User? user = _userRepository.FindById(userId);
            if (user == null)
            {
                // Just making sure, should never happen though
                return new List<OAuthLink>();
            }

            return _userRepository.GetLinkedProviders(userId);
        }

        public bool LinkOAuth(int userId, string provider)
        {
            throw new NotImplementedException();
        }

        public bool UnlinkOAuth(int userId, string provider)
        {
            throw new NotImplementedException();
        }

        public List<NotificationPreference> GetNotificationPreferences(int userId)
        {
            User? user = _userRepository.FindById(userId);
            if (user == null)
            {
                // AGAIN just making sure, should never happen
                return new List<NotificationPreference>();
            }

            return _userRepository.GetNotificationPreferences(userId);
        }

        public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs)
        {
            User? user = _userRepository.FindById(userId);
            if (user == null)
            {
                // Last time just making sure, should never happen
                return false;
            }

            return _userRepository.UpdateNotificationPreferences(userId, prefs);
        }

        public bool VerifyPassword(int userId, string password)
        {
            User? user = _userRepository.FindById(userId);

            if (user == null)
            {
                // ACTUAL last time making sure, should never happen though
                return false;
            }

            return _hashService.Verify(password, user.PasswordHash);
        }
    }
}