using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class GeneralCargoProductTariffController : Controller
    {
        private readonly IGeneralCargoProductTariffService _GeneralCargoProductTariffService;

        public GeneralCargoProductTariffController(IGeneralCargoProductTariffService GeneralCargoProductTariffService)
        {
            _GeneralCargoProductTariffService = GeneralCargoProductTariffService;
        }

        public async Task<IActionResult> List(GeneralCargoProductTariffFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _GeneralCargoProductTariffService.GetPageGeneralCargoProductTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateGeneralCargoProductTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await _GeneralCargoProductTariffService.CreateGeneralCargoProductTariff(create, token);
            return RedirectToAction(nameof(List), new { GeneralCargoProductId = create.GeneralCargoProductId });
        }

        public async Task<IActionResult> DeleteTariff(Guid GeneralCargoProductTariffId, CancellationToken token = default)
        {
            var generalCargoProductId = await _GeneralCargoProductTariffService.DeleteGeneralCargoProductTariff(GeneralCargoProductTariffId, token);
            return RedirectToAction(nameof(List), new { GeneralCargoProductId = generalCargoProductId });
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await _GeneralCargoProductTariffService.GetTariffById(tariffId, token);
            return PartialView("_CreateGeneralCargoProductTariff", tariff);
        }
    }
}
