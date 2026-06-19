using System.Threading;
using System.Threading.Tasks;
using Common.enumeration.Hirman;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;

namespace WebPanel.Controllers
{
    // Authorization is enforced globally via AuthorizeFilter in Startup
    [Authorize()]
    public class PaymentTestController : Controller
    {
        private readonly IPaymentReceiptService _receiptService;
        private readonly ITejaratElectronicParsianService _tejaratService;
        private readonly ICurrentUserService<UserPanelInfoDto> _currentUserPanelService;

        public PaymentTestController(
            IPaymentReceiptService receiptService,
            ITejaratElectronicParsianService tejaratService,
            ICurrentUserService<UserPanelInfoDto> currentUserPanelService
        )
        {
            _receiptService = receiptService;
            _tejaratService = tejaratService;
            _currentUserPanelService = currentUserPanelService;
        }

        [HttpGet]
        public async Task<IActionResult> Start(CancellationToken ct)
        {
            var userPanel = await _currentUserPanelService.GetCurrentUser();
            var userId = (int?)userPanel?.UserId;

            var init = await _receiptService.InitAsync(
                new PaymentReceiptInitInputDto
                {
                    UserId = userId,
                    PersonId = null,
                    Price = 10000,
                    PaymentReasonType = PaymentReasonType.Other,
                },
                ct
            );

            var trx = await _tejaratService.GenerateTransactionAsync(
                new TejaratElectronicParsianGenerateTransactionInputDto
                {
                    PaymentReceiptId = init.Id,
                    Price = 10000,
                    UserId = userId,
                    PersonId = null,
                },
                ct
            );

            if (string.IsNullOrWhiteSpace(trx?.PaymentUrl))
                return Json(trx);

            return Redirect(trx.PaymentUrl);
        }
    }
}
