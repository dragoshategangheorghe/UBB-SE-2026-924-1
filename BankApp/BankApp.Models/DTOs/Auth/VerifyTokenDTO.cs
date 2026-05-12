using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Auth
{
    public class VerifyTokenDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
