using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Profile
{
    public class UpdateProfileResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public UpdateProfileResponse()
        {
        }
        public UpdateProfileResponse(bool success, string? message)
        {
            Success = success;
            Message = message;
        }

        public override bool Equals(object? obj)
        {
            UpdateProfileResponse? other = obj as UpdateProfileResponse;

            return other != null &&
                   Success == other.Success &&
                   Message == other.Message;
        }
    }
}
