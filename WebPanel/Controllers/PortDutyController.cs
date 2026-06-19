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
    public class PortDutyController : Controller
    {
        private readonly IPortDutyService _portDutyService;
        private readonly IPortDutyTariffService _portDutyTariffService;

        public PortDutyController(IPortDutyService portDutyService, IPortDutyTariffService portDutyTariffService)
        {
            _portDutyService = portDutyService;
            _portDutyTariffService = portDutyTariffService;
        }

        public async Task<IActionResult> List(PortDutyFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _portDutyService.GetPagePortDuty(p ?? 1, filter ?? new PortDutyFilterDto(), token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreatePortDutyDto create, CancellationToken token = default)
        {
            await _portDutyService.CreatePortDuty(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid portDutyId, CancellationToken token = default)
        {
            await _portDutyService.DeletePortDuty(portDutyId, token);
            return RedirectToAction(nameof(List));
        }

        #region Port Duty Tariff
        public async Task<IActionResult> ListTariff(PortDutyTariffFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _portDutyTariffService.GetPagePortDutyTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreatePortDutyTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await _portDutyTariffService.CreatePortDutyTariff(create, token);
            return RedirectToAction(nameof(ListTariff), new { PortDutyId = create.PortDutyId });
        }

        public async Task<IActionResult> DeleteTariff(Guid portDutyTariffId, CancellationToken token = default)
        {
            var portDutyId = await _portDutyTariffService.DeletePortDutyTariff(portDutyTariffId, token);
            return RedirectToAction(nameof(ListTariff), new { PortDutyId = portDutyId });
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await _portDutyTariffService.GetTariffById(tariffId, token);
            return PartialView("_CreatePortDutyTariff", tariff);
        }

        #endregion
    }
}
