using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionHistoryService _transactionHistoryService;

        public TransactionsController(ITransactionHistoryService transactionHistoryService)
        {
            this._transactionHistoryService = transactionHistoryService;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"]!;

        [HttpGet("filters")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(_transactionHistoryService.GetFilterMetadata(GetAuthenticatedUserId()));
        }

        [HttpPost("history")]
        public IActionResult GetHistory([FromBody] TransactionHistoryRequest request)
        {
            return Ok(_transactionHistoryService.GetHistory(GetAuthenticatedUserId(), request));
        }

        [HttpGet("{transactionId:int}")]
        public IActionResult GetTransaction(int transactionId)
        {
            TransactionDetailsResponse response = _transactionHistoryService.GetTransaction(GetAuthenticatedUserId(), transactionId);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost("export")]
        public IActionResult ExportTransactions([FromBody] TransactionExportRequest request)
        {
            TransactionExportResult exportResult = _transactionHistoryService.ExportTransactions(GetAuthenticatedUserId(), request);
            return File(exportResult.Content, exportResult.ContentType, exportResult.FileName);
        }

        [HttpGet("{transactionId:int}/receipt")]
        public IActionResult ExportReceipt(int transactionId)
        {
            TransactionExportResult exportResult = _transactionHistoryService.ExportReceipt(GetAuthenticatedUserId(), transactionId);
            if (exportResult.Content.Length == 0)
            {
                return NotFound();
            }

            return File(exportResult.Content, exportResult.ContentType, exportResult.FileName);
        }
    }
}
