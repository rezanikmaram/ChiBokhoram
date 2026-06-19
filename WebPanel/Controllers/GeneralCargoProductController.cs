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
    public class GeneralCargoProductController : Controller
    {
        private readonly IGeneralCargoProductService _GeneralCargoProductService;

        public GeneralCargoProductController(IGeneralCargoProductService GeneralCargoProductService)
        {
            _GeneralCargoProductService = GeneralCargoProductService;
        }

        public async Task<IActionResult> List(GeneralCargoProductFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _GeneralCargoProductService.GetPageGeneralCargoProduct(p ?? 1, filter, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetGeneralCargoProductTitle(Guid? generalCargoProductTariffId, CancellationToken token = default)
        {
            if (generalCargoProductTariffId == null || generalCargoProductTariffId == Guid.Empty)
                return Json("مشخص نشده");

            var tariffTitle = (await _GeneralCargoProductService.GetTariffTitle(generalCargoProductTariffId.Value, token)) ?? "مشخص نشده";

            return Json(tariffTitle);
        }
    }
}
