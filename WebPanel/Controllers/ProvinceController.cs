using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Entities.DTOs.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class ProvinceController : Controller
    {
        private readonly IProvinceService _provinceService;

        public ProvinceController(IProvinceService provinceService)
        {
            _provinceService = provinceService ?? throw new ArgumentNullException(nameof(provinceService));
        }

        public async Task<IActionResult> Index(int? countryId, ProvinceFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            // Use filter.CountryId if provided, otherwise use countryId parameter
            var effectiveCountryId = filter?.CountryId ?? countryId;
            var result = await _provinceService.GetProvincePageList(p ?? 1, effectiveCountryId, filter, token);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Table(int? countryId, ProvinceFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            // Use filter.CountryId if provided, otherwise use countryId parameter
            var effectiveCountryId = filter?.CountryId ?? countryId;
            var result = await _provinceService.GetProvincePageList(p ?? 1, effectiveCountryId, filter, token);
            return PartialView("_ProvinceTable", result);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProvinceModal(int? provinceId, int? countryId, CancellationToken token = default)
        {
            var countries = await _provinceService.GetAllCountries(token);
            ViewBag.Countries = countries;

            if (provinceId.HasValue && provinceId.Value > 0)
            {
                var dto = await _provinceService.GetProvinceById(provinceId.Value, token);
                return PartialView("_ProvinceForm", dto);
            }

            var newDto = new ProvinceDto();
            if (countryId.HasValue && countryId.Value > 0)
            {
                newDto.CountryId = countryId.Value;
            }
            return PartialView("_ProvinceForm", newDto);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProvince(ProvinceDto dto, CancellationToken token = default)
        {
            if (!ModelState.IsValid)
            {
                var countries = await _provinceService.GetAllCountries(token);
                ViewBag.Countries = countries;
                return PartialView("_ProvinceForm", dto);
            }

            try
            {
                var result = await _provinceService.AddOrUpdate(dto, token);
                if (!string.IsNullOrEmpty(result.Error))
                {
                    return Json(new { success = false, message = result.Error });
                }
                return Json(new { success = true, message = "عملیات با موفقیت انجام شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProvince(int provinceId, CancellationToken token = default)
        {
            try
            {
                var msg = await _provinceService.DeleteById(provinceId, token);
                var message = JsonConvert.DeserializeObject<ResponseMessage>(msg);
                return Json(new { success = message.Type == ResponseMessageType.Success, message = message.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در حذف: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProvinceCount(int? countryId, CancellationToken token = default)
        {
            var count = await _provinceService.GetProvinceCount(countryId, token);
            return Json(new { count = count });
        }
    }
}
