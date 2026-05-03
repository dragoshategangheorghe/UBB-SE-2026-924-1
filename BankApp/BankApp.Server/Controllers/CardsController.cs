using BankApp.Models.DTOs.Cards;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService cardService;

        public CardsController(ICardService cardService)
        {
            this.cardService = cardService;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        [HttpGet]
        public IActionResult GetCards()
        {
            return Ok(cardService.GetCards(GetAuthenticatedUserId()));
        }

        [HttpGet("{cardId:int}")]
        public IActionResult GetCard(int cardId)
        {
            CardDetailsResponse response = cardService.GetCard(GetAuthenticatedUserId(), cardId);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost("{cardId:int}/reveal")]
        public IActionResult RevealCard(int cardId, [FromBody] RevealCardRequest request)
        {
            RevealCardResponse response = cardService.RevealSensitiveDetails(GetAuthenticatedUserId(), cardId, request);
            return response.Success || response.RequiresOtp ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{cardId:int}/freeze")]
        public IActionResult FreezeCard(int cardId)
        {
            CardCommandResponse response = cardService.FreezeCard(GetAuthenticatedUserId(), cardId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{cardId:int}/unfreeze")]
        public IActionResult UnfreezeCard(int cardId)
        {
            CardCommandResponse response = cardService.UnfreezeCard(GetAuthenticatedUserId(), cardId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{cardId:int}/settings")]
        public IActionResult UpdateSettings(int cardId, [FromBody] UpdateCardSettingsRequest request)
        {
            CardCommandResponse response = cardService.UpdateSettings(GetAuthenticatedUserId(), cardId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("preferences/sort")]
        public IActionResult UpdateSortPreference([FromBody] UpdateCardSortPreferenceRequest request)
        {
            CardCommandResponse response = cardService.UpdateSortPreference(GetAuthenticatedUserId(), request);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
