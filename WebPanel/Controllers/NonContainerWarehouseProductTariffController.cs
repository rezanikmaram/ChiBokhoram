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
    public class NonContainerWarehouseProductTariffController : Controller
    {
        private readonly INonContainerWarehouseProductTariffService _NonContainerWarehouseProductTariffService;

        public NonContainerWarehouseProductTariffController(INonContainerWarehouseProductTariffService NonContainerWarehouseProductTariffService)
        {
            _NonContainerWarehouseProductTariffService = NonContainerWarehouseProductTariffService;
        }

        public async Task<IActionResult> List(
            NonContainerWarehouseProductTariffFilterDto filter = null,
            CancellationToken token = default,
            int? p = null
        )
        {
            var data = await _NonContainerWarehouseProductTariffService.GetPageNonContainerWarehouseProductTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateNonContainerWarehouseProductTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await _NonContainerWarehouseProductTariffService.CreateNonContainerWarehouseProductTariff(create, token);
            return RedirectToAction(nameof(List), new { NonContainerWarehouseProductId = create.NonContainerWarehouseProductId });
        }

        public async Task<IActionResult> DeleteTariff(Guid nonContainerWarehouseProductTariffId, CancellationToken token = default)
        {
            var nonContainerWarehouseProductId = await _NonContainerWarehouseProductTariffService.DeleteNonContainerWarehouseProductTariff(
                nonContainerWarehouseProductTariffId,
                token
            );
            return RedirectToAction(nameof(List), new { NonContainerWarehouseProductId = nonContainerWarehouseProductId });
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await _NonContainerWarehouseProductTariffService.GetTariffById(tariffId, token);
            return PartialView("_CreateNonContainerWarehouseProductTariff", tariff);
        }
    }
}
