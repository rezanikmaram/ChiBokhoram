using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class SpecialGearTariffRuleController : Controller
    {
        private readonly ISpecialGearTariffRuleService _specialGearTariffRuleService;

        public SpecialGearTariffRuleController(ISpecialGearTariffRuleService specialGearTariffRuleService)
        {
            _specialGearTariffRuleService = specialGearTariffRuleService;
        }

        public async Task<IActionResult> List(SpecialGearTariffRuleFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _specialGearTariffRuleService.GetPageSpecialGearTariffRule(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariffRule(CreateSpecialGearTariffRuleDto create, CancellationToken token = default)
        {
            TempData["msg"] = await _specialGearTariffRuleService.CreateSpecialGearTariffRule(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> DeleteTarrifRule(Guid specialGearTariffRuleId, CancellationToken token = default)
        {
            await _specialGearTariffRuleService.DeleteSpecialGearTariffRule(specialGearTariffRuleId, token);
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffRuleById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await _specialGearTariffRuleService.GetTariffRuleById(tariffId, token);
            return PartialView("_CreateSpecialGearTariffRule", tariff);
        }
    }
}
