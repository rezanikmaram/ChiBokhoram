using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class SpecialGearTariffController : Controller
    {
        private readonly ISpecialGearTariffService _specialGearTariffService;

        public SpecialGearTariffController(ISpecialGearTariffService specialGearTariffService)
        {
            _specialGearTariffService = specialGearTariffService;
        }

        public async Task<IActionResult> List(SpecialGearTariffFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _specialGearTariffService.GetPageSpecialGearTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateSpecialGearTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await _specialGearTariffService.CreateSpecialGearTariff(create, token);
            return RedirectToAction(nameof(List), new { SpecialGearTariffRuleId = create.SpecialGearTariffRuleId });
        }

        public async Task<IActionResult> DeleteTariff(Guid specialGearTariffId, CancellationToken token = default)
        {
            var speciaGearTariffRuleId = await _specialGearTariffService.DeleteSpecialGearTariff(specialGearTariffId, token);
            return RedirectToAction(nameof(List), new { SpecialGearTariffRuleId = speciaGearTariffRuleId });
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await _specialGearTariffService.GetSpecialGearTariffById(tariffId, token);
            return PartialView("_CreateSpecialGearTariff", tariff);
        }
    }
}
