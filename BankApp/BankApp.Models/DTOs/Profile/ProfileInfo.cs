using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.Entities;

namespace BankApp.Models.DTOs.Profile
{
    public class ProfileInfo
    {
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public bool Is2FAEnabled { get; set; }

        public ProfileInfo()
        {
        }

        public ProfileInfo(User user)
        {
            if (user != null)
            {
                UserId = user.Id;
                Email = user.Email;
                FullName = user.FullName;
                PhoneNumber = user.PhoneNumber;
                Address = user.Address;
                Nationality = user.Nationality;
                Is2FAEnabled = user.Is2FAEnabled;
            }
        }
    }
}
