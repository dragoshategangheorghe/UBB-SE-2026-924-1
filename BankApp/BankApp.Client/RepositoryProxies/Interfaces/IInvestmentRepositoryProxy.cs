using BankApp.Models.Features.Investments;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Interfaces
{
    public interface IInvestmentRepositoryProxy
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);

        Task<IActionResult> TradeAsync(dynamic tradeData);
    }
}
