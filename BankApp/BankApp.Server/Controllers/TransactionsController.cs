using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionHistoryRepository transactionHistoryRepository;

        public TransactionsController(ITransactionHistoryRepository transactionHistoryRepository)
        {
            this.transactionHistoryRepository = transactionHistoryRepository;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        // /api/transactions
        public IActionResult GetTransactionsByUser()
        {
            return Ok(transactionHistoryRepository.GetTransactionsByUserId(GetAuthenticatedUserId()));
        }

        [HttpGet("{transactionId:int}")]
        public IActionResult GetTransaction(int transactionId)
        {
            TransactionHistoryItemDto transaction = transactionHistoryRepository.GetTransactionById(GetAuthenticatedUserId(), transactionId);
            return transaction == null ? Ok(transaction) : NotFound(transaction);
        }

        [HttpGet("cards")] // why do we need this for user cards?? so backwards, anyway
        public IActionResult GetCards()
        {
            return Ok(transactionHistoryRepository.GetCardsByUserId(GetAuthenticatedUserId()));
        }

        [HttpGet("accounts")]
        public IActionResult GetAccounts()
        {
            return Ok(transactionHistoryRepository.GetAccountsByUserId(GetAuthenticatedUserId()));
        }

    }
}
