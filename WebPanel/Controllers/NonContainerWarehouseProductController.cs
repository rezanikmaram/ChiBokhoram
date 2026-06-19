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
    public class NonContainerWarehouseProductController : Controller
    {
        private readonly INonContainerWarehouseProductService _NonContainerWarehouseProductService;

        public INonContainerWarehouseProductDiscountService NonContainerWarehouseProductDiscountService { get; }

        public NonContainerWarehouseProductController(
            INonContainerWarehouseProductService NonContainerWarehouseProductService,
            INonContainerWarehouseProductDiscountService nonContainerWarehouseProductDiscountService
        )
        {
            _NonContainerWarehouseProductService = NonContainerWarehouseProductService;
            NonContainerWarehouseProductDiscountService = nonContainerWarehouseProductDiscountService;
        }

        public async Task<IActionResult> List(NonContainerWarehouseProductFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _NonContainerWarehouseProductService.GetPageNonContainerWarehouseProduct(p ?? 1, filter, token);
            return View(data);
        }
         
		[HttpPost]
		public async Task<IActionResult> GetNonContainerWarehouseProductTitle(Guid? nonContainerWarehouseProductTariffId, CancellationToken token = default)
		{
			if (nonContainerWarehouseProductTariffId == null || nonContainerWarehouseProductTariffId == Guid.Empty)
				return Json("مشخص نشده");

			var tariffTitle = (await _NonContainerWarehouseProductService.GetTariffTitle(nonContainerWarehouseProductTariffId.Value, token)) ?? "مشخص نشده";

			return Json(tariffTitle);
		}

		#region تخفیف ها
		[HttpPost]
        public async Task<IActionResult> GetDiscountPage(CancellationToken token = default)
        {
            var discount = await NonContainerWarehouseProductDiscountService.GetDiscountPage(token);
            return PartialView("_DiscountPage", discount);
        }

        [HttpPost]
        public async Task<IActionResult> GetDiscountCreate(int? discountId, CancellationToken token = default)
        {
            var discount = await NonContainerWarehouseProductDiscountService.GetDiscountCreate(discountId, token);
            return PartialView("_CreateDiscount", discount);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscount(
            CreateNonContainerWarehouseProductDiscountDto createDiscount,
            CancellationToken token = default
        )
        {
            await NonContainerWarehouseProductDiscountService.CreateDiscount(createDiscount, token);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDiscount(int discountId, CancellationToken token = default)
        {
            await NonContainerWarehouseProductDiscountService.DeleteDiscount(discountId, token);

            return Ok();
        }

        #endregion
    }
}
