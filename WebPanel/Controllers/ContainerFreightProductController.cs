using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class ContainerFreightProductController : Controller
    {
        private readonly IContainerFreightProductService _containerFreightProductService;
        private readonly IContainerFreightProductTariffService containerFreightProductTariffService;

        public ContainerFreightProductController(
            IContainerFreightProductService containerFreightProductService,
            IContainerFreightProductTariffService containerFreightProductTariffService
        )
        {
            _containerFreightProductService = containerFreightProductService;
            this.containerFreightProductTariffService = containerFreightProductTariffService;
        }

        public async Task<IActionResult> List(ContainerFreightProductFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _containerFreightProductService.GetPageContainerFreightProduct(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreateContainerFreightProductDto create, CancellationToken token = default)
        {
            await _containerFreightProductService.CreateContainerFreightProduct(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid ContainerFreightProductId, CancellationToken token = default)
        {
            await _containerFreightProductService.DeleteContainerFreightProduct(ContainerFreightProductId, token);
            return RedirectToAction(nameof(List));
        }

        #region Tariff
        public async Task<IActionResult> ListTariff(
            ContainerFreightProductTariffFilterDto filter = null,
            CancellationToken token = default,
            int? p = null
        )
        {
            var data = await containerFreightProductTariffService.GetPageContainerFreightProductTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateContainerFreightProductTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await containerFreightProductTariffService.CreateContainerFreightProductTariff(create, token);
            return RedirectToAction(nameof(ListTariff), new { ContainerFreightProductId = create.ContainerFreightProductId });
        }

        public async Task<IActionResult> DeleteTariff(Guid containerFreightProductTariffId, CancellationToken token = default)
        {
            try
            {
                var ContainerFreightProductId = await containerFreightProductTariffService.DeleteContainerFreightProductTariff(
                    containerFreightProductTariffId,
                    token
                );
                return RedirectToAction(nameof(ListTariff), new { ContainerFreightProductId = ContainerFreightProductId });
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
