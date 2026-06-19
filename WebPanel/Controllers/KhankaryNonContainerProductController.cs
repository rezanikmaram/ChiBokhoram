using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using Services.WebService.Concrete;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class KhankaryNonContainerProductController : Controller
    {
        private readonly IKhankaryNonContainerProductService _KhankaryNonContainerProductService;
        private readonly IKhankaryNonContainerProductTariffService _KhankaryNonContainerProductTariffService;

        public KhankaryNonContainerProductController(IKhankaryNonContainerProductService KhankaryNonContainerProductService,
                IKhankaryNonContainerProductTariffService khankaryNonContainerProductTariffService)
        {
            _KhankaryNonContainerProductService = KhankaryNonContainerProductService;
            _KhankaryNonContainerProductTariffService = khankaryNonContainerProductTariffService;
        }

        public async Task<IActionResult> List(KhankaryNonContainerProductFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _KhankaryNonContainerProductService.GetPageKhankaryNonContainerProduct(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreateKhankaryNonContainerProductDto create, CancellationToken token = default)
        {
            await _KhankaryNonContainerProductService.CreateKhankaryNonContainerProduct(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid KhankaryNonContainerProductId, CancellationToken token = default)
        {
            await _KhankaryNonContainerProductService.DeleteKhankaryNonContainerProduct(KhankaryNonContainerProductId, token);
            return RedirectToAction(nameof(List));
        }

        #region تعرفه خن کاری غیر کانتینری
        public async Task<IActionResult> ListTariff(KhankaryNonContainerProductTariffFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _KhankaryNonContainerProductTariffService.GetPageKhankaryNonContainerProductTariff(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> CreateTariff(CreateKhankaryNonContainerProductTariffDto create, CancellationToken token = default)
        {
            await _KhankaryNonContainerProductTariffService.CreateKhankaryNonContainerProductTariff(create, token);
            return RedirectToAction(nameof(ListTariff), new { KhankaryNonContainerProductId = create.KhankaryNonContainerProductId });
        }

        public async Task<IActionResult> DeleteTariff(Guid KhankaryNonContainerProductTariffId, CancellationToken token = default)
        {
            var KhankaryNonContainerProductId = await _KhankaryNonContainerProductTariffService.DeleteKhankaryNonContainerProductTariff(KhankaryNonContainerProductTariffId, token);
            return RedirectToAction(nameof(ListTariff), new { KhankaryNonContainerProductId = KhankaryNonContainerProductId });
        }
        #endregion
    }
}
