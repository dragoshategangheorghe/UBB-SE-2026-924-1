using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Profile
{
    // DEPRECATED:
    // Currently deprecated as the GetProfile endpoint incorporates its only property, the userId, in the URL
    // Not deleted yet as the endpoint request might be changed in the future to encapsulate more information
    [Obsolete]
    public class GetProfileRequest
    {
        public int UserId { get; set; }

        public GetProfileRequest(int userId)
        {
            UserId = userId;
        }
    }
}
