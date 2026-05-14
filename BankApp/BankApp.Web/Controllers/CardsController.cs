using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Cards;
using BankApp.Web.Models.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    //[Authorize]
    public class CardsController : Controller
    {
        private readonly ICardService _cardService;
        private readonly ApiService _apiService;

        public CardsController(ICardService cardService, ApiService apiService)
        {
            _cardService = cardService;
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ApplyBearerToken();

            GetCardsResponse? response = await _cardService.GetCardsAsync();

            CardManagementPageViewModel model = new CardManagementPageViewModel();

            if (response?.Success == true)
            {
                model.Cards = response.Cards.ToList();
                model.SelectedSortOption = response.SortOption;
                model.SelectedCard = model.Cards.FirstOrDefault();
            }
            else
            {
                model.StatusMessage = "Failed to load cards.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSort([FromBody] UpdateCardSortPreferenceRequest request)
        {
            ApplyBearerToken();

            CardCommandResponse? response = await _cardService.UpdateSortPreferenceAsync(request);

            return Json(response ?? new CardCommandResponse
            {
                Success = false,
                Message = "Sort update failed."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Freeze([FromBody] CardIdRequest request)
        {
            ApplyBearerToken();

            CardCommandResponse? response = await _cardService.FreezeCardAsync(request.CardId);

            return Json(response ?? new CardCommandResponse
            {
                Success = false,
                Message = "Freeze failed."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unfreeze([FromBody] CardIdRequest request)
        {
            ApplyBearerToken();

            CardCommandResponse? response = await _cardService.UnfreezeCardAsync(request.CardId);

            return Json(response ?? new CardCommandResponse
            {
                Success = false,
                Message = "Unfreeze failed."
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings([FromBody] CardSettingsRequest request)
        {
            ApplyBearerToken();

            UpdateCardSettingsRequest apiRequest = new UpdateCardSettingsRequest
            {
                SpendingLimit = request.SpendingLimit,
                IsOnlinePaymentsEnabled = request.IsOnlinePaymentsEnabled,
                IsContactlessPaymentsEnabled = request.IsContactlessPaymentsEnabled
            };

            CardCommandResponse? response = await _cardService.UpdateSettingsAsync(request.CardId, apiRequest);

            return Json(response ?? new CardCommandResponse
            {
                Success = false,
                Message = "Settings update failed."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Reveal([FromBody] RevealCardPageRequest request)
        {
            ApplyBearerToken();

            RevealCardRequest apiRequest = new RevealCardRequest
            {
                Password = request.Password,
                OtpCode = request.OtpCode
            };

            RevealCardResponse? response = await _cardService.RevealCardAsync(request.CardId, apiRequest);

            return Json(response ?? new RevealCardResponse
            {
                Success = false,
                Message = "Reveal failed."
            });
        }

        //private void ApplyBearerToken()
        //{
        //    string? token = HttpContext.Session.GetString("AuthToken");

        //    if (!string.IsNullOrWhiteSpace(token))
        //    {
        //        _apiService.SetToken(token);
        //    }
        //}
        private void ApplyBearerToken()
        {
            string? token = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwiZXhwIjoxNzc5MzkxMDc0fQ.qRdG-1nnSaJ_-AOm2Powdsn3lG0GImD6vbr0A_uXPjs";
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                _apiService.SetToken(token);
            }
        }

    }

    public class CardIdRequest
    {
        public int CardId { get; set; }
    }

    public class CardSettingsRequest
    {
        public int CardId { get; set; }
        public decimal? SpendingLimit { get; set; }
        public bool? IsOnlinePaymentsEnabled { get; set; }
        public bool? IsContactlessPaymentsEnabled { get; set; }
    }

    public class RevealCardPageRequest
    {
        public int CardId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? OtpCode { get; set; }
    }
}
