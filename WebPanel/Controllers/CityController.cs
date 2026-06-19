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
    public class CityController : Controller
    {
        private readonly ICityService _cityService;

        public CityController(ICityService cityService)
        {
            _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        }

        public async Task<IActionResult> Index(int? provinceId, CityFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            // Use filter.ProvinceId if provided, otherwise use provinceId parameter
            var effectiveProvinceId = filter?.ProvinceId ?? provinceId;
            var result = await _cityService.GetCityPageList(p ?? 1, effectiveProvinceId, filter, token);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Table(int? provinceId, CityFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            // Use filter.ProvinceId if provided, otherwise use provinceId parameter
            var effectiveProvinceId = filter?.ProvinceId ?? provinceId;
            var result = await _cityService.GetCityPageList(p ?? 1, effectiveProvinceId, filter, token);
            return PartialView("_CityTable", result);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCityModal(int? cityId, int? provinceId, CancellationToken token = default)
        {
            // Load all provinces with country names for the combined dropdown
            var provinces = await _cityService.GetAllProvincesWithCountry(token);
            ViewBag.Provinces = provinces;

            if (cityId.HasValue && cityId.Value > 0)
            {
                var dto = await _cityService.GetCityById(cityId.Value, token);
                return PartialView("_CityForm", dto);
            }

            var newDto = new CityDto();
            if (provinceId.HasValue && provinceId.Value > 0)
            {
                newDto.ProvinceId = provinceId.Value;
            }
            return PartialView("_CityForm", newDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetProvincesByCountry(int countryId, CancellationToken token = default)
        {
            var provinces = await _cityService.GetProvincesByCountryId(countryId, token);
            return Json(provinces);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCity(CityDto dto, CancellationToken token = default)
        {
            if (!ModelState.IsValid)
            {
                // Load all provinces with country names for the combined dropdown
                var provinces = await _cityService.GetAllProvincesWithCountry(token);
                ViewBag.Provinces = provinces;

                return PartialView("_CityForm", dto);
            }

            try
            {
                var result = await _cityService.AddOrUpdate(dto, token);
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
        public async Task<IActionResult> DeleteCity(int cityId, CancellationToken token = default)
        {
            try
            {
                var msg = await _cityService.DeleteById(cityId, token);
                var message = JsonConvert.DeserializeObject<ResponseMessage>(msg);
                return Json(new { success = message.Type == ResponseMessageType.Success, message = message.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در حذف: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCityCount(int? provinceId, CancellationToken token = default)
        {
            var count = await _cityService.GetCityCount(provinceId, token);
            return Json(new { count = count });
        }
    }
}
