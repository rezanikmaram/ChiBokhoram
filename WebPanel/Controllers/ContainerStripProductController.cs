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
    public class ContainerStripProductController : Controller
    {
        private readonly IContainerStripProductService _containerStripProductService;
        private readonly IContainerStripProductTariffService containerStripProductTariffService;

        public ContainerStripProductController(
            IContainerStripProductService containerStripProductService,
            IContainerStripProductTariffService containerStripProductTariffService
        )
        {
            _containerStripProductService = containerStripProductService;
            this.containerStripProductTariffService = containerStripProductTariffService;
        }

        public async Task<IActionResult> List(ContainerStripProductFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _containerStripProductService.GetPageContainerStripProduct(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreateContainerStripProductDto create, CancellationToken token = default)
        {
            await _containerStripProductService.CreateContainerStripProduct(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid ContainerStripProductId, CancellationToken token = default)
        {
            await _containerStripProductService.DeleteContainerStripProduct(ContainerStripProductId, token);
            return RedirectToAction(nameof(List));
        }

        #region Tariff
        public async Task<IActionResult> ListTariff(
            ContainerStripProductTariffFilterDto filter = null,
            CancellationToken token = default,
            int? p = null
        )
        {
            var data = await containerStripProductTariffService.GetPageContainerStripProductTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateContainerStripProductTariffDto create, CancellationToken token = default)
        {
            TempData["msg"] = await containerStripProductTariffService.CreateContainerStripProductTariff(create, token);
            return RedirectToAction(nameof(ListTariff), new { ContainerStripProductId = create.ContainerStripProductId });
        }

        public async Task<IActionResult> DeleteTariff(Guid containerStripProductTariffId, CancellationToken token = default)
        {
            try
            {
                var ContainerStripProductId = await containerStripProductTariffService.DeleteContainerStripProductTariff(
                    containerStripProductTariffId,
                    token
                );
                return RedirectToAction(nameof(ListTariff), new { ContainerStripProductId = ContainerStripProductId });
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
