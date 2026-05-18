using BankApp.Client.RepoProxies;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.DTOs.Transactions;
using BankApp.Web.Models.Transactions;
using Microsoft.AspNetCore.Mvc;

//[Authorize]
namespace BankApp.Web.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionHistoryService _transactionService;
        private readonly ApiService _apiService;

        public TransactionsController(ITransactionHistoryService transactionService, ApiService apiService)
        {
            _transactionService = transactionService;
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            TransactionHistoryPageViewModel model = new TransactionHistoryPageViewModel();
            TransactionHistoryRequest request = new TransactionHistoryRequest();
            TransactionHistoryResponse? response = await _transactionService.GetHistoryAsync(request);

            if (response?.Success == true)
            {
                model.Transactions = response.Transactions;
                //model.AppliedFilters = response.AppliedFilters;
            }
            else
            {
                model.Transactions = new List<TransactionHistoryItemDto>();
                model.StatusMessage = response?.Message ?? "Failed to load transactions.";
            }

            return View(model);
            
        }


    }
}
