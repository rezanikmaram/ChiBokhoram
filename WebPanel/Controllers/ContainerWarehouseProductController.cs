using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class ContainerWarehouseProductController : Controller
    {
        private readonly IContainerWarehouseProductService _ContainerWarehouseProductService;
        private readonly IContainerWarehouseProductTariffService containerWarehouseProductTariffService;

        public ContainerWarehouseProductController(
            IContainerWarehouseProductService ContainerWarehouseProductService,
            IContainerWarehouseProductTariffService containerWarehouseProductTariffService
        )
        {
            _ContainerWarehouseProductService = ContainerWarehouseProductService;
            this.containerWarehouseProductTariffService = containerWarehouseProductTariffService;
        }

        public async Task<IActionResult> List(ContainerWarehouseProductFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _ContainerWarehouseProductService.GetPageContainerWarehouseProduct(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreateContainerWarehouseProductDto create, CancellationToken token = default)
        {
            TempData["msg"] = await _ContainerWarehouseProductService.CreateContainerWarehouseProduct(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid ContainerWarehouseProductId, CancellationToken token = default)
        {
            await _ContainerWarehouseProductService.DeleteContainerWarehouseProduct(ContainerWarehouseProductId, token);
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await _ContainerWarehouseProductService.GetTariffById(tariffId, token);
            return PartialView("_CreateContainerWarehouseProduct", tariff);
        }

        #region تعرفه انبارداری کانتینری
        public async Task<IActionResult> ListTariff(
            ContainerWarehouseProductTariffFilterDto filter = null,
            CancellationToken token = default,
            int? p = null
        )
        {
            var data = await containerWarehouseProductTariffService.GetPageContainerWarehouseProductTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateContainerWarehouseProductTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await containerWarehouseProductTariffService.CreateContainerWarehouseProductTariff(create, token);
            return RedirectToAction(nameof(ListTariff), new { ContainerWarehouseProductId = create.ContainerWarehouseProductId });
        }

        [HttpPost]
        public async Task<IActionResult> GetTariffSubById(Guid tariffId, CancellationToken token = default)
        {
            var tariff = await containerWarehouseProductTariffService.GetTariffById(tariffId, token);
            return PartialView("_CreateContainerWarehouseProductTariff", tariff);
        }

        public async Task<IActionResult> DeleteTariff(Guid ContainerWarehouseProductTariffId, CancellationToken token = default)
        {
            try
            {
                var ContainerWarehouseProductId = await containerWarehouseProductTariffService.DeleteContainerWarehouseProductTariff(
                    ContainerWarehouseProductTariffId,
                    token
                );
                return RedirectToAction(nameof(ListTariff), new { ContainerWarehouseProductId = ContainerWarehouseProductId });
            }
            catch (Exception ex)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, ex.Message));
                return RedirectToAction(nameof(List));
            }
        }
        #endregion
    }
}
