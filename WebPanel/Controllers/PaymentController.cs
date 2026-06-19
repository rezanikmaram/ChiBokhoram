using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;

namespace WebPanel.Controllers
{
    [AllowAnonymous]
    public class PaymentController : Controller
    {
        private readonly ITejaratElectronicParsianService _tejaratService;

        public PaymentController(ITejaratElectronicParsianService tejaratService)
        {
            _tejaratService = tejaratService;
        }

        [HttpPost("api/payment/paymentResult")]
        [HttpGet("api/payment/paymentResult")]
        public async Task<IActionResult> PaymentResult([FromForm] string Token, [FromQuery] string token, CancellationToken ct)
        {
            var tok = string.IsNullOrWhiteSpace(Token) ? token : Token;
            if (string.IsNullOrWhiteSpace(tok))
                return BadRequest("Token is required");

            var result = await _tejaratService.ConfirmAsync(new PaymentReceiptConfirmInputDto { Token = tok }, ct);
            return Json(result);
        }
    }
}
