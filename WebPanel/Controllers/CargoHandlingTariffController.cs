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
    public class CargoHandlingTariffController : Controller
    {
        private readonly ICargoHandlingTariffService _CargoHandlingTariffService;

        public CargoHandlingTariffController(ICargoHandlingTariffService cargoHandlingTariffService)
        {
            _CargoHandlingTariffService = cargoHandlingTariffService;
        }

        public async Task<IActionResult> List(CargoHandlingTariffFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _CargoHandlingTariffService.GetPageCargoHandling(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreateCargoHandlingTariffDto createCargoHandling, CancellationToken token = default)
        {
            TempData["msg"] = await _CargoHandlingTariffService.CreateCargoHandlingTariff(createCargoHandling, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid baseCargoHandlingId, CancellationToken token = default)
        {
            await _CargoHandlingTariffService.DeleteCargoHandling(baseCargoHandlingId, token);
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> GetBaseCargoHandlingById(Guid baseCargoHandlingId, CancellationToken token = default)
        {
            var tariff = await _CargoHandlingTariffService.GetBaseCargoHandlingById(baseCargoHandlingId, token);
            return PartialView("_CreateCargoHandlingTariff", tariff);
        }
    }
}
