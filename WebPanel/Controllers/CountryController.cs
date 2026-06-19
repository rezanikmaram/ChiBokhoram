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
    public class CountryController : Controller
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        }

        public async Task<IActionResult> Index(CountryFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await _countryService.GetCountryPageList(p ?? 1, filter, token);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Table(CountryFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await _countryService.GetCountryPageList(p ?? 1, filter, token);
            return PartialView("_CountryTable", result);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCountryModal(int? countryId, CancellationToken token = default)
        {
            if (countryId.HasValue && countryId.Value > 0)
            {
                var dto = await _countryService.GetCountryById(countryId.Value, token);
                return PartialView("_CountryForm", dto);
            }

            return PartialView("_CountryForm", new CountryDto());
        }

        [HttpPost]
        public async Task<IActionResult> SaveCountry(CountryDto dto, CancellationToken token = default)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CountryForm", dto);
            }

            try
            {
                var result = await _countryService.AddOrUpdate(dto, token);
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
        public async Task<IActionResult> DeleteCountry(int countryId, CancellationToken token = default)
        {
            try
            {
                var msg = await _countryService.DeleteById(countryId, token);
                var message = JsonConvert.DeserializeObject<ResponseMessage>(msg);
                return Json(new { success = message.Type == ResponseMessageType.Success, message = message.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در حذف: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCountryCount(CancellationToken token = default)
        {
            var count = await _countryService.GetCountryCount(token);
            return Json(new { count = count });
        }
    }
}
