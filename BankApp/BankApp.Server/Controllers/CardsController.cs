using BankApp.Models.DTOs.Cards;
using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardRepository cardRepository;

        public CardsController(ICardRepository cardRepository)
        {
            this.cardRepository = cardRepository;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        [HttpGet]
        public IActionResult GetCards()
        {
            return Ok(cardRepository.GetCardsByUserId(GetAuthenticatedUserId()));
        }

        [HttpGet("{cardId:int}")]
        public ActionResult<Card> GetCard(int cardId)
        {
            return cardRepository.GetCardById(cardId);
        }

        [HttpGet("account/{accountId:int}")]
        public ActionResult<Account> GetAccount(int accountId)
        {
            return cardRepository.GetAccountById(accountId);
        }

        [HttpGet("/sortPreference")]
        public ActionResult<UserCardPreference> GetSortPreference()
        {
            return cardRepository.GetSortPreference(GetAuthenticatedUserId());
        }

        [HttpPut("/sortPreference/{sortOption}")]
        public IActionResult SaveSortPreferences(string sortOption)
        {
            return Ok(cardRepository.SaveSortPreference(GetAuthenticatedUserId(), sortOption));
        }

        [HttpPut("{cardId: int}/updateStatus/{status}")]
        public IActionResult UpdateStatus(int cardId, string status)
        {
            return Ok(cardRepository.UpdateStatus(cardId, status));
        }

        [HttpPost("{cardId: int}/updateSettings")]
        public IActionResult UpdateStatus(int cardId, [FromBody] UpdateCardSettingsRequest request)
        {
            return Ok(cardRepository.UpdateSettings(cardId, request.SpendingLimit, request.IsOnlinePaymentsEnabled, request.IsContactlessPaymentsEnabled));
        }
    }
}
