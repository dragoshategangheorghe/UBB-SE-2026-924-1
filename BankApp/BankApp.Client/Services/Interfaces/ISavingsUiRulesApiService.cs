using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsUiRulesApiService
    {
        decimal ParsePositiveAmount(string text);

        string GetDepositPreview(DepositPreviewRequest request);
    }
}
